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
import org.gdgsp.adapter.CardAdapter;
import org.gdgsp.lib.HeaderViewRecyclerAdapter;
import org.gdgsp.model.Event;
import org.gdgsp.other.Other;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;

/**
 * Fragment da aba eventos antigos.
 */
public class PastEventsFragment extends Fragment {
    private AppCompatActivity activity;
    private View view;

    private ViewGroup footer;
    private View footer_refresh, footer_message;
    private TextView footer_message_text;

    private HeaderViewRecyclerAdapter hv;
    private CardAdapter cardAdapter;
    private RecyclerView list;
    private LinearLayoutManager linearLayoutManager;
    public ProgressBar progress;

    private Gson gson = new Gson();

    public static List<Event> listEvents;

    public LinearLayout errorScreen;
    private TextView errorMessage;

    private boolean loading = false;
    private int pastVisiblesItems, visibleItemCount, totalItemCount;

    private int page = 1;
    private boolean events_error = false;

    private static PastEventsFragment pastEventsFragment = null;

    public static PastEventsFragment getInstance() {
        return pastEventsFragment;
    }

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        this.activity = (AppCompatActivity)getActivity();
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        setHasOptionsMenu(true);
        pastEventsFragment = this;
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        view = inflater.inflate(R.layout.content_main, container, false);

        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

        errorScreen = (LinearLayout)view.findViewById(R.id.error_screen);
        errorMessage = (TextView)view.findViewById(R.id.error_message);

        linearLayoutManager = new LinearLayoutManager(activity);
        list.setLayoutManager(linearLayoutManager);

        listEvents = new ArrayList<Event>();
        cardAdapter = new CardAdapter(activity, listEvents, true);
        hv = new HeaderViewRecyclerAdapter(cardAdapter);
        list.setAdapter(hv);

        footer = (ViewGroup)inflater.inflate(R.layout.footer_message_refresh, list, false);

        footer_refresh = footer.findViewById(R.id.refresh);
        footer_message = footer.findViewById(R.id.card);
        footer_message_text = (TextView)footer_message.findViewById(R.id.message);

        hv.addFooterView(footer);

        footer_refresh.setVisibility(View.VISIBLE);

        footer_message.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View p1) {
                if(events_error) {
                    events_error = false;
                    footer_refresh.setVisibility(View.VISIBLE);
                    footer_message.setVisibility(View.GONE);
                    getEvents(page);
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
                            if(listEvents.size() > 0) {
                                loading = true;

                                getEvents(page);
                            }
                        }
                    }
                }
            }
        });

        view.findViewById(R.id.try_again).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View p1) {
                EventsFragment.getInstance().reconnect();
            }
        });
        return view;
    }

    /**
     * Método que solicita a lista de eventos e preenche a lista.
     */
    private void getEvents(int page) {
        this.page = page;

        Ion.with(this)
                .load(Other.getEventsUrl(activity, page))
                .setBodyParameter("refresh_token", Other.getRefreshToken(activity))
                .asJsonObject()
                .setCallback(new FutureCallback<JsonObject>() {
                    @Override
                    public void onCompleted(Exception e, final JsonObject json) {
                        if(e != null) {
                            if(e instanceof JsonParseException) {
                                errorMessage.setText(getString(R.string.error_message));
                                errorScreen.setVisibility(View.VISIBLE);
                            } else {
                                loading = true;
                                events_error = true;
                                footer_refresh.setVisibility(View.GONE);
                                footer_message_text.setText(getString(R.string.try_again));
                                footer_message.setVisibility(View.VISIBLE);
                            }
                            progress.setVisibility(View.GONE);
                            return;
                        }

                        hv.notifyDataSetChanged();

                        if(json.get("more_past_events").getAsBoolean()) {
                            loading = false;
                            footer_refresh.setVisibility(View.VISIBLE);
                        } else {
                            loading = true;
                            footer_refresh.setVisibility(View.GONE);
                            footer_message_text.setText(getString(R.string.events_no_more));
                            footer_message.setVisibility(View.VISIBLE);
                        }

                        events_error = false;

                        formList(json, "");
                    }
                });
    }

    public void formList(JsonObject json, String error) {
        if(json == null) {
            errorMessage.setText(error);
            errorScreen.setVisibility(View.VISIBLE);
            progress.setVisibility(View.GONE);
            return;
        }

        PastEventsFragment.this.page++;

        Type datasetListType = new TypeToken<List<Event>>() {}.getType();

        List<Event> listNewEvents = gson.fromJson(json.get("past_events").getAsJsonArray().toString(), datasetListType);

        listEvents.addAll(listNewEvents);

        hv.notifyDataSetChanged();

        progress.setVisibility(View.GONE);
        list.setVisibility(View.VISIBLE);
    }
}