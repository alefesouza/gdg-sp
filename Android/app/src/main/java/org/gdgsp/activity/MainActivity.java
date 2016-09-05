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

package org.gdgsp.activity;

import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.design.widget.NavigationView;
import android.support.design.widget.TabLayout;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.GravityCompat;
import android.support.v4.view.ViewPager;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.ion.Ion;
import com.onesignal.OneSignal;

import org.gdgsp.R;
import org.gdgsp.adapter.TabAdapter;
import org.gdgsp.fragment.EventFragment;
import org.gdgsp.fragment.EventsFragment;
import org.gdgsp.fragment.PastEventsFragment;
import org.gdgsp.fragment.TweetsFragment;
import org.gdgsp.model.Event;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;

import java.lang.reflect.Type;

/**
 * Activity principal do aplicativo, contém o navigation drawer e a lista de eventos.
 */
public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private SharedPreferences preferences;
    public static NavigationView navigationView;
    private DrawerLayout drawer;
    public static Person person = null;
    private Gson gson = new Gson();

	private TabLayout tabLayout;
	private ViewPager viewPager;
	private TabAdapter adapter;

    public static View navHeader;
    public static TextView profileName, profileIntro;
    public static ImageView profilePhoto;

    public static int openEvent = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        preferences = PreferenceManager.getDefaultSharedPreferences(this);

        drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(
			this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.setDrawerListener(toggle);
        toggle.syncState();

        navigationView = (NavigationView) findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);
        drawer = (DrawerLayout) findViewById(R.id.drawer_layout);

        navHeader = navigationView.getHeaderView(0);

        profileName = (TextView)navHeader.findViewById(R.id.profile_name);
        profileIntro = (TextView)navHeader.findViewById(R.id.profile_intro);
        profilePhoto = (ImageView)navHeader.findViewById(R.id.profile_photo);

        if(preferences.contains("member_profile")) {
            Type datasetListType = new TypeToken<Person>() {}.getType();
            person = gson.fromJson(preferences.getString("member_profile", ""), datasetListType);

            profileName.setText(person.getName());

            profileIntro.setText(person.getIntro());

            Ion.with(MainActivity.this).load(person.getPhoto()).intoImageView(profilePhoto);
        }

        if(getIntent().hasExtra("fromlogin")) {
            Other.showToast(this, getString(R.string.login_success));
            getIntent().removeExtra("fromlogin");
        }

        navigationView.getHeaderView(0).setOnClickListener(new OnClickListener() {
				@Override
				public void onClick(View p1) {
					if(person == null) {
						Intent intent = new Intent(MainActivity.this, FragmentActivity.class);
						intent.putExtra("fragment", 2);
						intent.putExtra("title", getString(R.string.login_do));
						intent.putExtra("url", Other.getLoginUrl(MainActivity.this));
						intent.putExtra("islogin", true);
						startActivity(intent);
					} else {
						Other.openSite(MainActivity.this, "http://meetup.com/" + getString(R.string.meetup_id) + "/members/" + person.getId());
						drawer.closeDrawer(GravityCompat.START);
					}
				}
			});

		viewPager = (ViewPager) findViewById(R.id.viewpager);
		viewPager.setOffscreenPageLimit(3);
		adapter = new TabAdapter(getSupportFragmentManager());
		viewPager.setAdapter(adapter);

		tabLayout = (TabLayout) findViewById(R.id.sliding_tabs);
		tabLayout.setupWithViewPager(viewPager);

		adapter.addFragment(new EventsFragment(), getString(R.string.comming));
		adapter.addFragment(new PastEventsFragment(), getString(R.string.before));
		adapter.addFragment(new TweetsFragment(), getString(R.string.hashtag));

		adapter.notifyDataSetChanged();
    }

    /**
     * Método que abre o evento selecionado, criando uma Activity no mobile, ou abrindo na coluna direita no tablet.
     * @param event Evento a ser aberto.
     */
    public static void openEvent(AppCompatActivity activity, Event event, boolean isPast) {
        if (!Other.isTablet(activity)) {
            Intent intent = new Intent(activity, FragmentActivity.class);
            intent.putExtra("fragment", 1);
            intent.putExtra("event", event);
			intent.putExtra("isPast", isPast);
            activity.startActivity(intent);
        } else {
            FragmentTransaction ft = activity.getSupportFragmentManager().beginTransaction();
            Fragment eventFragment = new EventFragment();
            Bundle bundle = new Bundle();
            bundle.putSerializable("event", event);
			bundle.putBoolean("isPast", isPast);
            eventFragment.setArguments(bundle);
            ft.replace(R.id.frame_event, eventFragment);
            ft.commit();
        }
    }

    @Override
    public void onBackPressed() {
        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(GravityCompat.START);
        } else {
            super.onBackPressed();
        }
    }

    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.nav_notification:
                Intent notification = new Intent(MainActivity.this, FragmentActivity.class);
                notification.putExtra("fragment", 5);
                startActivity(notification);
                break;
            case R.id.nav_raffle:
				String[] types = {getString(R.string.comming), getString(R.string.before)};
				
				AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
				LayoutInflater inflater = (LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE);
				View convertView = inflater.inflate(R.layout.list_pressed, null);
				alertDialog.setView(convertView);
				alertDialog.setTitle(getString(R.string.choose_event_type));

				alertDialog.setItems(types, new DialogInterface.OnClickListener() {
						public void onClick(DialogInterface dialog, int item) {
							showRaffleDialog(item != 0);
						}
					});

				alertDialog.create();
				alertDialog.show();
                break;
            case R.id.nav_site:
                Other.openSite(this, getString(R.string.site_url));
                break;
            case R.id.nav_old_meetups:
                Other.openSite(this, getString(R.string.old_meetups_url));
                break;
            case R.id.nav_facebook:
                Other.openSite(this, getString(R.string.facebook_url));
                break;
            case R.id.nav_googleplus:
                Other.openSite(this, getString(R.string.googleplus_url));
                break;
            case R.id.nav_instagram:
                Other.openSite(this, getString(R.string.instagram_url));
                break;
            case R.id.nav_twitter:
                Other.openSite(this, getString(R.string.twitter_url));
                break;
            case R.id.nav_youtube:
                Other.openSite(this, getString(R.string.youtube_url));
                break;
            case R.id.nav_contact:
                Intent intent = new Intent(Intent.ACTION_VIEW);
                Uri data = Uri.parse("mailto:" + getString(R.string.contact_mail));
                intent.setData(data);
                startActivity(intent);
                break;
            case R.id.nav_settings:
                if(Build.VERSION.SDK_INT >= 11) {
                    Intent settings = new Intent(MainActivity.this, FragmentActivity.class);
                    settings.putExtra("fragment", 0);
                    startActivity(settings);
                } else {
                    AlertDialog alertDialog2 = new AlertDialog.Builder(this)
						.setTitle(getString(R.string.pref_notification))
						.setMessage(getString(R.string.pref_notification_summary))
						.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								OneSignal.setSubscription(true);
								p1.dismiss();
							}
						})
						.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
							@Override
							public void onClick(DialogInterface p1, int p2) {
								OneSignal.setSubscription(false);
								p1.dismiss();
							}
						})
						.create();

                    alertDialog2.show();
                }
                break;
            case R.id.nav_about:
                Intent about = new Intent(MainActivity.this, FragmentActivity.class);
                about.putExtra("fragment", 3);
                startActivity(about);
                break;
        }

        drawer.closeDrawer(GravityCompat.START);
        return true;
    }

	public void showRaffleDialog(final boolean past) {
		final EventsFragment eventsFragment = EventsFragment.getInstance();
		final PastEventsFragment pastEventsFragment = PastEventsFragment.getInstance();

		String[] events = new String[0];

		if(past) {
			if(pastEventsFragment.listEvents.size() > 0) {
				events = new String[pastEventsFragment.listEvents.size()];
			}

			for (int i = 0; i < pastEventsFragment.listEvents.size(); i++) {
				events[i] = pastEventsFragment.listEvents.get(i).getName();
			}
		} else {
			if(eventsFragment.listEvents.size() > 0) {
				events = new String[eventsFragment.listEvents.size()];
			}

			for (int i = 0; i < eventsFragment.listEvents.size(); i++) {
				events[i] = eventsFragment.listEvents.get(i).getName();
			}
		}

		if(events.length == 0) {
			Toast.makeText(this, getString(R.string.no_events), Toast.LENGTH_SHORT);
			return;
		}

		AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
		LayoutInflater inflater = (LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		View convertView = inflater.inflate(R.layout.list_pressed, null);
		alertDialog.setView(convertView);
		alertDialog.setTitle(getString(R.string.choose_event));

		alertDialog.setItems(events, new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog, int item) {
					int id;
					
					if(past) {
						id = pastEventsFragment.getInstance().listEvents.get(item).getId();
					} else {
						id = eventsFragment.getInstance().listEvents.get(item).getId();
					}
					
					Intent raffleManager = new Intent(MainActivity.this, FragmentActivity.class);
					raffleManager.putExtra("fragment", 7);
					raffleManager.putExtra("eventid", id);
					startActivity(raffleManager);
				}
			});

		alertDialog.create();
		alertDialog.show();
	}
}
