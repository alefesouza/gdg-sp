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
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v4.app.Fragment;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnKeyListener;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import org.gdgsp.R;
import org.gdgsp.activity.MainActivity;
import org.gdgsp.other.Other;

/**
 * Fragment onde é exibido páginas web.
 */
public class WebViewFragment extends Fragment {
    private AppCompatActivity activity;
    private View view;

	private WebView webView;
	private ProgressBar progress;

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

        webView = (WebView)view.findViewById(R.id.webview);
		progress = (ProgressBar)view.findViewById(R.id.progress);

        webView.setWebViewClient(new webViewClient());
		webView.getSettings().setJavaScriptEnabled(true);

		webView.loadUrl(getArguments().getString("url"));

		webView.setOnKeyListener(new OnKeyListener() {
				@SuppressWarnings("static-access")
				public boolean onKey(View view, int keyCode, KeyEvent event) {
					if (event.getAction() == event.ACTION_DOWN) {
						if (keyCode == KeyEvent.KEYCODE_BACK) {
							if (webView.canGoBack()) {
								webView.goBack();
							} else {
								getActivity().finish();
							}
							return true;
						}
					}
					return true;
				}
			});
			
		return view;
	}

    public class webViewClient extends WebViewClient {
        @Override
        public boolean shouldOverrideUrlLoading(WebView view, String url) {
			if(getArguments().getBoolean("islogin") && url.contains(getString(R.string.backend_url))) {
				if(url.contains("refresh_token")) {
					String token = Other.getQuery(url.replace("#", "?"), "refresh_token");
					if(!token.equals("")) {
						SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(activity);
						SharedPreferences.Editor editor = preferences.edit();
						editor.putString("refresh_token", token);
						editor.commit();
						
						Intent intent = new Intent(activity, MainActivity.class);
						intent.putExtra("fromlogin", true);
						if(activity.getIntent().hasExtra("eventid")) {
							MainActivity.openEvent = activity.getIntent().getIntExtra("eventid", 0);
						}
						intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
						startActivity(intent);
					}
				} else if(url.contains("error")) {
					AlertDialog alertDialog = new AlertDialog.Builder(activity)
						.setTitle(getString(R.string.login_error))
						.setMessage(getString(R.string.login_error_sub))
						.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								webView.loadUrl(Other.getLoginUrl(activity));
							}
						})
						.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								activity.finish();
							}
						})
						.create();

					alertDialog.show();
				} else if(url.contains("nonmember=none")) {
					AlertDialog alertDialog = new AlertDialog.Builder(activity)
						.setMessage(getString(R.string.login_nonmember).replace("{meetupid}", getString(R.string.meetup_id)))
						.setPositiveButton(getString(android.R.string.yes), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								getArguments().remove("islogin");
								webView.loadUrl("http://meetup.com/" + getString(R.string.meetup_id));
							}
						})
						.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								activity.finish();
							}
						})
						.create();

					alertDialog.show();
				} else if(url.contains("nonmember=pending")) {
					AlertDialog alertDialog = new AlertDialog.Builder(activity)
						.setMessage(getString(R.string.login_pending).replace("{meetupid}", getString(R.string.meetup_id)))
						.setPositiveButton(getString(android.R.string.yes), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								activity.finish();
							}
						})
						.create();

					alertDialog.show();
				}
			}

			activity.supportInvalidateOptionsMenu();
			view.loadUrl(url);
            return super.shouldOverrideUrlLoading(view, url);
        }

		@Override
		public void onPageFinished(WebView view, String url) {
			try {
				if (!url.contains(getString(R.string.backend_url))) {
					activity.getSupportActionBar().setTitle(view.getTitle());
					activity.getSupportActionBar().setSubtitle(url);
					progress.setVisibility(View.GONE);
					webView.setVisibility(View.VISIBLE);
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
			super.onPageFinished(view, url);
		}
    }

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		inflater = getActivity().getMenuInflater();
		inflater.inflate(R.menu.webview, menu);

		menu.findItem(R.id.menu_share).setVisible(!getArguments().getBoolean("islogin", false));
		menu.findItem(R.id.menu_open).setVisible(!getArguments().getBoolean("islogin", false));

		super.onCreateOptionsMenu(menu, inflater);
	}

	@Override
	public void onPrepareOptionsMenu(Menu menu) {
		menu.findItem(R.id.menu_forward).setEnabled(webView.canGoForward());

		super.onPrepareOptionsMenu(menu);
	}

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
			case R.id.menu_open:
				Intent intent = new Intent(Intent.ACTION_VIEW);
				intent.setData(Uri.parse(webView.getUrl()));
				startActivity(intent);
				return true;
			case R.id.menu_forward:
				if(webView.canGoForward()) {
					webView.goForward();
				}
				return true;
			case R.id.menu_refresh:
				webView.reload();
				return true;
			case R.id.menu_share:
				Intent sharePageIntent = new Intent();
				sharePageIntent.setAction(Intent.ACTION_SEND);
				sharePageIntent.putExtra(Intent.EXTRA_TEXT, webView.getTitle() + " " + webView.getUrl().replace("?m=1", ""));
				sharePageIntent.setType("text/plain");
				startActivity(Intent.createChooser(sharePageIntent, getResources().getText(R.string.share)));
				return true;
			case R.id.menu_cache:
				webView.clearCache(true);
				Other.showToast(activity, getString(R.string.cache2));
				return true;
            default:
                return
					super.onOptionsItemSelected(item);
        }
    }
}
