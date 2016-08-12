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
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Build;
import android.support.v4.app.Fragment;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.design.widget.FloatingActionButton;
import android.os.Bundle;
import android.os.Handler;
import android.preference.PreferenceManager;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MenuInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.ProgressBar;
import org.gdgsp.R;
import org.gdgsp.activity.FragmentActivity;
import org.gdgsp.activity.PeopleActivity;
import org.gdgsp.model.Event;
import org.gdgsp.other.Other;

/**
 * Fragment onde é exibido o evento selecionado.
 */
public class EventFragment extends Fragment {
	private AppCompatActivity activity;
	private View view;

	private WebView webView;
	private ProgressBar progress;

	private Event event;
	private SharedPreferences preferences;

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
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.fragment_webview, container, false);
		preferences = PreferenceManager.getDefaultSharedPreferences(activity);

		event = (Event)getArguments().getSerializable("event");

		webView = (WebView)view.findViewById(R.id.webview);
		progress = (ProgressBar)view.findViewById(R.id.progress);

		webView.getSettings().setJavaScriptEnabled(true);
		webView.setWebViewClient(new webViewClient());
		webView.loadData(event.getDescription(), "text/html; charset=utf-8", "utf-8");

		// Tempo para carregar o conteúdo sem mostrar aquela "animação" da WebView
		new Handler().postDelayed(new Runnable() {
			@Override
			public void run() {
				progress.setVisibility(View.GONE);
				webView.setVisibility(View.VISIBLE);
			}
		}, 1000);

		FloatingActionButton do_rsvp = (FloatingActionButton)view.findViewById(R.id.do_rsvp);
		do_rsvp.setVisibility(View.VISIBLE);
		do_rsvp.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View p1) {
				if(preferences.contains("refresh_token")) {
					if(event.isRsvpable() || (event.getResponse() != null && event.getResponse().matches("yes|waitlist"))) {
						Intent rsvp = new Intent(activity, FragmentActivity.class);
						rsvp.putExtra("fragment", 4);
						rsvp.putExtra("event", event);
						startActivity(rsvp);
					} else {
						Other.showMessage(activity, "", getString(R.string.rsvp_cant));
					}
				} else {
					AlertDialog alertDialog = new AlertDialog.Builder(activity)
							.setTitle(getString(R.string.rsvp_need))
							.setMessage(getString(R.string.rsvp_need_sub))
							.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface p1, int p2) {
									openLogin();
								}
							})
							.setNegativeButton(getString(R.string.no), null)
							.create();

					alertDialog.show();
				}
			}
		});

		return view;
	}

	private void openLogin() {
		Intent intent = new Intent(activity, FragmentActivity.class);
		intent.putExtra("fragment", 2);
		intent.putExtra("eventid", event.getId());
		intent.putExtra("title", getString(R.string.login_do));
		intent.putExtra("url", Other.getLoginUrl(activity));
		intent.putExtra("islogin", true);
		startActivity(intent);
	}

	private class webViewClient extends WebViewClient {
		@Override
		public boolean shouldOverrideUrlLoading(WebView view, String url) {
			view.stopLoading();
			if(url.startsWith("http://do_login")) {
				openLogin();
			} else if(url.startsWith("geo:")) {
				Intent intent = new Intent(Intent.ACTION_VIEW);
				intent.setData(Uri.parse(url));
				startActivity(intent);
			} else {
				Other.openSite(activity, url);
			}
			return super.shouldOverrideUrlLoading(view, url);
		}
	}

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		inflater = getActivity().getMenuInflater();
		inflater.inflate(R.menu.menu_event, menu);

		menu.findItem(R.id.menu_people).setTitle(event.getWho());

		super.onCreateOptionsMenu(menu, inflater);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
			case R.id.menu_share:
				Intent sharePageIntent = new Intent();
				sharePageIntent.setAction(Intent.ACTION_SEND);
				sharePageIntent.putExtra(Intent.EXTRA_TEXT, event.getName() + " " + event.getLink());
				sharePageIntent.setType("text/plain");
				startActivity(Intent.createChooser(sharePageIntent, getResources().getText(R.string.share)));
				return true;
			case R.id.menu_people:
				Intent intent = new Intent(activity, PeopleActivity.class);
				intent.putExtra("id", event.getId());
				intent.putExtra("who", event.getWho());
				startActivity(intent);
				return true;
			case R.id.menu_copylink:
				if(Build.VERSION.SDK_INT <= 10) {
					android.text.ClipboardManager clipboard = (android.text.ClipboardManager)activity.getSystemService(Context.CLIPBOARD_SERVICE);
					clipboard.setText(event.getLink());
				} else {
					ClipboardManager clipboardManager = (ClipboardManager)activity.getSystemService(Context.CLIPBOARD_SERVICE);
					ClipData clipData = ClipData.newPlainText(event.getLink(), event.getLink());
					clipboardManager.setPrimaryClip(clipData);
				}
				Other.showToast(activity, getString(R.string.link_copyed));
				return true;
			case R.id.menu_openinbrowser:
				Other.openSite(activity, event.getLink());
				return true;
			default:
				return
						super.onOptionsItemSelected(item);
		}
	}
}
