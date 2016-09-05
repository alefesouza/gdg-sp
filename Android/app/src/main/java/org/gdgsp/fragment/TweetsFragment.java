/*
 * Copyright (C) 2016 Alefe Souza <http://alefesouza.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.gdgsp.fragment;

import android.app.Activity;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v4.app.Fragment;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;

import org.gdgsp.R;
import org.gdgsp.adapter.TweetAdapter;
import org.gdgsp.lib.HeaderViewRecyclerAdapter;
import org.gdgsp.model.Tweet;
import org.gdgsp.other.Other;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;

/**
 * Fragment da aba tweets.
 */
public class TweetsFragment extends Fragment {
	private AppCompatActivity activity;
	private View view;

    private TweetAdapter tweetAdapter;
    private RecyclerView list;
	private LinearLayoutManager linearLayoutManager;
    private ProgressBar progress;

	private ViewGroup footer;
	private View footer_refresh, footer_message;
	private TextView footer_message_text;

	private HeaderViewRecyclerAdapter hv;
	
	private String max_id;
	private boolean tweets_error = false;
	
	private Gson gson = new Gson();

    private List<Tweet> listTweets;

    private LinearLayout errorScreen;
    private TextView errorMessage;

	private boolean loading = false;
	private int pastVisiblesItems, visibleItemCount, totalItemCount;

	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = (AppCompatActivity)getActivity();
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		setHasOptionsMenu(true);
		super.onCreate(savedInstanceState);
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, 	Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.content_main, container, false);

        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

        errorScreen = (LinearLayout)view.findViewById(R.id.error_screen);
        errorMessage = (TextView)view.findViewById(R.id.error_message);

		linearLayoutManager = new LinearLayoutManager(activity);
        list.setLayoutManager(linearLayoutManager);

		listTweets = new ArrayList<Tweet>();
		tweetAdapter = new TweetAdapter(activity, listTweets);
		hv = new HeaderViewRecyclerAdapter(tweetAdapter);
		list.setAdapter(hv);

		footer = (ViewGroup)inflater.inflate(R.layout.footer_message_refresh, list, false);

		footer_refresh = footer.findViewById(R.id.refresh);
		footer_message = footer.findViewById(R.id.card);
		footer_message_text = (TextView)footer_message.findViewById(R.id.message);

		hv.addFooterView(footer);
		
		footer_message.setOnClickListener(new OnClickListener() {
				@Override
				public void onClick(View p1) {
					if(tweets_error) {
						tweets_error = false;
						footer_refresh.setVisibility(View.VISIBLE);
						footer_message.setVisibility(View.GONE);
						getTweets(max_id);
					}
				}
		});

		list.addOnScrollListener(new RecyclerView.OnScrollListener() {
				@Override
				public void onScrolled(RecyclerView recyclerView, int dx, int dy) {
					if(dy > 0) {
						visibleItemCount = linearLayoutManager.getChildCount();
						totalItemCount = linearLayoutManager.getItemCount();
						pastVisiblesItems = linearLayoutManager.findFirstVisibleItemPosition();

						if (!loading) {
							if ((visibleItemCount + pastVisiblesItems) >= totalItemCount) {
								if(listTweets.size() > 0) {
									loading = true;

									getTweets(max_id);
								}
							}
						}
					}
				}
			});

        view.findViewById(R.id.try_again).setOnClickListener(new OnClickListener() {
				@Override
				public void onClick(View p1) {
					errorScreen.setVisibility(View.GONE);
					getTweets("0");
				}
			});

        getTweets("0");
		return view;
	}

    /**
     * Método que solicita a lista de tweets e preenche a lista.
     */
    private void getTweets(String max_id) {
		if(max_id.equals("0")) {
			list.setVisibility(View.GONE);
			progress.setVisibility(View.VISIBLE);
		}

        Ion.with(this)
			.load(Other.getTweetsUrl(activity, max_id))
			.asJsonObject()
			.setCallback(new FutureCallback<JsonObject>() {
				@Override
				public void onCompleted(Exception e, JsonObject json) {
					if(e != null) {
						if(e instanceof JsonParseException) {
							errorMessage.setText(getString(R.string.error_message));
							errorScreen.setVisibility(View.VISIBLE);
						} else {
							if(listTweets.size() > 0) {
								loading = true;
								tweets_error = true;;
								footer_refresh.setVisibility(View.GONE);
								footer_message_text.setText(getString(R.string.try_again));
								footer_message.setVisibility(View.VISIBLE);
							} else {
								errorMessage.setText(getString(R.string.connection_error));
								errorScreen.setVisibility(View.VISIBLE);
							}
						}
						progress.setVisibility(View.GONE);
						e.printStackTrace();
						return;
					}

					TweetsFragment.this.max_id = json.get("max_id").getAsString();

					if(json.get("more_tweets").getAsBoolean()) {
						loading = false;
						footer_refresh.setVisibility(View.VISIBLE);
					} else {
						loading = true;
						footer_refresh.setVisibility(View.GONE);
						footer_message_text.setText(getString(R.string.tweets_no_more));
						footer_message.setVisibility(View.VISIBLE);
					}
					
					Type datasetListType = new TypeToken<List<Tweet>>() {}.getType();

					List<Tweet> listNewTweets = gson.fromJson(json.get("tweets").getAsJsonArray().toString(), datasetListType);

					listTweets.addAll(listNewTweets);

					progress.setVisibility(View.GONE);

					list.setVisibility(View.VISIBLE);

					hv.notifyDataSetChanged();

					tweets_error = false;
					progress.setVisibility(View.GONE);
				}
			});
    }

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		inflater = getActivity().getMenuInflater();
		inflater.inflate(R.menu.menu_tweets, menu);

		super.onCreateOptionsMenu(menu, inflater);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
			case R.id.menu_refresh:
				listTweets.clear();
				footer_message.setVisibility(View.GONE);
                getTweets("0");
                return true;
			default:
				return
					super.onOptionsItemSelected(item);
		}
	}
}
