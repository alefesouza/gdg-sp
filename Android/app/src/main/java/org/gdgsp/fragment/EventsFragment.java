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
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v4.app.Fragment;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import com.onesignal.OneSignal;

import org.gdgsp.R;
import org.gdgsp.activity.FragmentActivity;
import org.gdgsp.activity.MainActivity;
import org.gdgsp.adapter.CardAdapter;
import org.gdgsp.model.Event;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;

import java.lang.reflect.Type;
import java.util.List;

/**
 * Fragment da aba eventos futuros.
 */
public class EventsFragment extends Fragment {
	private AppCompatActivity activity;
	private View view;
	
    private SharedPreferences preferences;
    private SharedPreferences.Editor editor;
	
    private CardAdapter cardAdapter;
    private RecyclerView list;
    private ProgressBar progress;
	
	private Gson gson = new Gson();

    public List<Event> listEvents;
	
    private LinearLayout errorScreen;
    private TextView errorMessage;

	private static EventsFragment eventsFragment = null;
	
	public static EventsFragment getInstance() {
		return eventsFragment;
	}
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = (AppCompatActivity)getActivity();
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		setHasOptionsMenu(true);
		eventsFragment = this;
		super.onCreate(savedInstanceState);
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, 	Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.content_main, container, false);

        preferences = PreferenceManager.getDefaultSharedPreferences(activity);
        editor = preferences.edit();
		
        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

        errorScreen = (LinearLayout)view.findViewById(R.id.error_screen);
        errorMessage = (TextView)view.findViewById(R.id.error_message);

        list.setLayoutManager(new LinearLayoutManager(activity));
		
        view.findViewById(R.id.try_again).setOnClickListener(new View.OnClickListener() {
				@Override
				public void onClick(View p1) {
                    reconnect();
				}
			});
			
        getEvents();
		return view;
	}


    /**
     * Método que solicita a lista de eventos e preenche a lista.
     */
    public void getEvents() {
        list.setVisibility(View.GONE);
        progress.setVisibility(View.VISIBLE);

        Ion.with(this)
			.load(Other.getEventsUrl(activity, 0))
			.setBodyParameter("refresh_token", Other.getRefreshToken(activity))
			.asJsonObject()
			.setCallback(new FutureCallback<JsonObject>() {
				@Override
				public void onCompleted(Exception e, final JsonObject json) {
					if(e != null) {
						String error;

						if(e instanceof JsonParseException) {
							error = getString(R.string.error_message);
						} else {
							error = getString(R.string.connection_error);
						}

						errorMessage.setText(error);
						errorScreen.setVisibility(View.VISIBLE);
						progress.setVisibility(View.GONE);
                        PastEventsFragment.getInstance().formList(null, error);
						return;
					}

					OneSignal.sendTag("app_version", Other.getAppVersion(activity));

					if(json.get("member").getAsJsonObject().get("id").getAsInt() > 0) {
						JsonObject member = json.get("member").getAsJsonObject();

						editor.putString("member_profile", member.toString()).commit();

						Type datasetListType = new TypeToken<Person>() {}.getType();
						MainActivity.person = gson.fromJson(json.get("member").getAsJsonObject().toString(), datasetListType);

						MainActivity.profileName.setText(MainActivity.person.getName());
						MainActivity.profileIntro.setText(MainActivity.person.getIntro());

						Ion.with(activity).load(MainActivity.person.getPhoto()).intoImageView(MainActivity.profilePhoto);

						if(member.get("is_admin").getAsBoolean()) {
							MainActivity.navigationView.getMenu().getItem(0).setVisible(true);
							MainActivity.navigationView.getMenu().getItem(1).setVisible(true);
						}

						OneSignal.sendTag("member_id", String.valueOf(MainActivity.person.getId()));
					} else {
						// Se o id retornar 0 e o usuário tiver a configuração refresh_token significa que tem algum problema com o token dele, nesse caso apaga o token atual e pede login novamente
						if(preferences.contains("refresh_token")) {
							MainActivity.profileName.setText(getString(R.string.login_do));
							MainActivity.profileIntro.setText("");
							MainActivity.profilePhoto.setImageResource(R.drawable.ic_launcher);

							MainActivity.person = null;

							editor.remove("refresh_token").remove("member_profile").remove("qr_code").commit();
						}
					}

					Type datasetListType = new TypeToken<List<Event>>() {}.getType();

					listEvents = gson.fromJson(json.get("events").getAsJsonArray().toString(), datasetListType);

					Ion.with(activity).load(json.get("header").getAsString()).intoImageView((ImageView)MainActivity.navHeader.findViewById(R.id.cover_photo));

					if(listEvents.size() > 0) {
						cardAdapter = new CardAdapter(activity, listEvents, false);
						list.setAdapter(cardAdapter);

						progress.setVisibility(View.GONE);

						if(MainActivity.openEvent != 0) {
							for(Event event : listEvents) {
								if(event.getId() == MainActivity.openEvent) {
									MainActivity.openEvent(activity, event, false);
									MainActivity.openEvent = 0;
								}
							}
						} else if(Other.isTablet(activity) && listEvents.size() > 0) {
							MainActivity.openEvent(activity, listEvents.get(0), false);
						}

						list.setVisibility(View.VISIBLE);
					} else {
						errorMessage.setText(getString(R.string.error_noevents));
						errorScreen.setVisibility(View.VISIBLE);
					}

					progress.setVisibility(View.GONE);

					PastEventsFragment.getInstance().formList(json, "");
				}
			});
        suggestLogin();
    }

    /**
     * Método que sugere ao usuário fazer login
     */
    private void suggestLogin() {
        if(!preferences.contains("suggest_login")) {
            AlertDialog alertDialog = new AlertDialog.Builder(activity)
				.setTitle(getString(R.string.suggest_login_title).replace("{appname}", getString(R.string.app_name)))
				.setMessage(getString(R.string.suggest_login_sub))
				.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
					@Override
					public void onClick(DialogInterface p1, int p2) {
						Intent intent = new Intent(activity, FragmentActivity.class);
						intent.putExtra("fragment", 2);
						intent.putExtra("title", getString(R.string.login_do));
						intent.putExtra("url", Other.getLoginUrl(activity));
						intent.putExtra("islogin", true);
						startActivity(intent);

						p1.dismiss();
					}
				})
				.setNegativeButton(getString(R.string.no), null)
				.create();

            alertDialog.show();

            editor.putBoolean("suggest_login", true);
            editor.commit();
        }
    }

    public void reconnect() {
        errorScreen.setVisibility(View.GONE);
        PastEventsFragment.getInstance().errorScreen.setVisibility(View.GONE);
        PastEventsFragment.getInstance().progress.setVisibility(View.VISIBLE);
        getEvents();
    }

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		inflater = getActivity().getMenuInflater();
		inflater.inflate(R.menu.menu_main, menu);
		
		super.onCreateOptionsMenu(menu, inflater);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
			case R.id.menu_refresh:
                getEvents();
                return true;
            case R.id.menu_checkin:
                if(preferences.contains("qr_code")) {
                    Intent checkin = new Intent(activity, FragmentActivity.class);
                    checkin.putExtra("fragment", 6);
                    startActivity(checkin);
                } else if(preferences.contains("refresh_token")) {
                    // Caso o usuário esteja com uma versão antiga do app
                    Intent intent = new Intent(activity, FragmentActivity.class);
                    intent.putExtra("fragment", 2);
                    intent.putExtra("title", getString(R.string.getting_qrcode));
                    intent.putExtra("url", Other.getLoginUrl(activity));
                    intent.putExtra("islogin", true);
                    startActivity(intent);

                    Other.showToast(activity, getString(R.string.getting_qrcode));
                } else {
                    AlertDialog alertDialog = new AlertDialog.Builder(activity)
						.setTitle(getString(R.string.checkin_need_login))
						.setMessage(getString(R.string.rsvp_need_sub))
						.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								Intent intent = new Intent(activity, FragmentActivity.class);
								intent.putExtra("fragment", 2);
								intent.putExtra("title", getString(R.string.login_do));
								intent.putExtra("url", Other.getLoginUrl(activity));
								intent.putExtra("islogin", true);
								startActivity(intent);

								p1.dismiss();
							}
						})
						.setNegativeButton(getString(R.string.no), null)
						.create();

                    alertDialog.show();
                }
                return true;
            case R.id.menu_open_meetup:
                Other.openMeetupApp(activity, "http://meetup.com/" + getString(R.string.meetup_id));
                return true;
			default:
				return
					super.onOptionsItemSelected(item);
		}
	}
}
