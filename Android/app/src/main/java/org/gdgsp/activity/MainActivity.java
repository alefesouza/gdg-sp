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

import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.design.widget.NavigationView;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
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
import java.lang.reflect.Type;
import java.util.List;
import org.gdgsp.R;
import org.gdgsp.adapter.CardAdapter;
import org.gdgsp.fragment.EventFragment;
import org.gdgsp.model.Event;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;

/**
 * Activity principal do aplicativo, contém o navigation drawer e a lista de eventos.
 */
public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private CardAdapter cardAdapter;
    private RecyclerView list;
    private ProgressBar progress;
    private SharedPreferences preferences;
    private SharedPreferences.Editor editor;
    private NavigationView navigationView;
    private DrawerLayout drawer;
    private Person person = null;
    private Gson gson = new Gson();

    public static List<Event> listEvents;

    private LinearLayout errorScreen;
    private View navHeader;
    private TextView profileName, profileIntro, errorMessage;
    private ImageView profilePhoto;

    public static int openEvent = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        preferences = PreferenceManager.getDefaultSharedPreferences(this);
        editor = preferences.edit();

        list = (RecyclerView)findViewById(R.id.list);
        progress = (ProgressBar)findViewById(R.id.progress);

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

        errorScreen = (LinearLayout)findViewById(R.id.error_screen);
        errorMessage = (TextView)findViewById(R.id.error_message);

        list.setLayoutManager(new LinearLayoutManager(MainActivity.this));

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

        findViewById(R.id.try_again).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View p1) {
                errorScreen.setVisibility(View.GONE);
                getEvents();
            }
        });

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

        getEvents();
    }

    /**
     * Método que solicita a lista de eventos e preenche a lista.
     */
    private void getEvents() {
        list.setVisibility(View.GONE);
        progress.setVisibility(View.VISIBLE);

        Ion.with(this)
                .load(Other.getEventsUrl(this))
                .setBodyParameter("refresh_token", Other.getRefreshToken(this))
                .asJsonObject()
                .setCallback(new FutureCallback<JsonObject>() {
                    @Override
                    public void onCompleted(Exception e, final JsonObject json) {
                        if(e != null) {
                            if(e instanceof JsonParseException) {
                                errorMessage.setText(getString(R.string.error_message));
                                errorScreen.setVisibility(View.VISIBLE);
                            } else {
                                errorMessage.setText(getString(R.string.connection_error));
                                errorScreen.setVisibility(View.VISIBLE);
                            }
                            progress.setVisibility(View.GONE);
                            return;
                        }

                        OneSignal.sendTag("app_version", Other.getAppVersion(MainActivity.this));

                        if(json.get("member").getAsJsonObject().get("id").getAsInt() > 0) {
                            JsonObject member = json.get("member").getAsJsonObject();

                            editor.putString("member_profile", member.toString()).commit();

                            Type datasetListType = new TypeToken<Person>() {}.getType();
                            person = gson.fromJson(json.get("member").getAsJsonObject().toString(), datasetListType);

                            profileName.setText(person.getName());
                            profileIntro.setText(person.getIntro());

                            Ion.with(MainActivity.this).load(person.getPhoto()).intoImageView(profilePhoto);

                            if(member.get("is_admin").getAsBoolean()) {
                                navigationView.getMenu().getItem(0).setVisible(true);
                            }

                            OneSignal.sendTag("member_id", String.valueOf(person.getId()));
                        } else {
                            // Se o id retornar 0 e o usuário tiver a configuração refresh_token significa que tem algum problema com o token dele, nesse caso apaga o token atual e pede login novamente
                            if(preferences.contains("refresh_token")) {
                                profileName.setText(getString(R.string.login_do));
                                profileIntro.setText("");
                                profilePhoto.setImageResource(R.drawable.ic_launcher);

                                person = null;

                                editor.remove("refresh_token").remove("member_profile").commit();
                            }
                        }

                        Type datasetListType = new TypeToken<List<Event>>() {}.getType();

                        listEvents = gson.fromJson(json.get("events").getAsJsonArray().toString(), datasetListType);

                        Ion.with(MainActivity.this).load(json.get("header").getAsString()).intoImageView((ImageView)navHeader.findViewById(R.id.cover_photo));

                        if(listEvents.size() > 0) {
                            cardAdapter = new CardAdapter(MainActivity.this, listEvents);
                            list.setAdapter(cardAdapter);

                            progress.setVisibility(View.GONE);

                            if(openEvent != 0) {
                                for(Event event : listEvents) {
                                    if(event.getId() == openEvent) {
                                        openEvent(event);
                                        openEvent = 0;
                                    }
                                }
                            } else if(Other.isTablet(MainActivity.this) && listEvents.size() > 0) {
                                openEvent(listEvents.get(0));
                            }

                            list.setVisibility(View.VISIBLE);
                        } else {
                            errorMessage.setText(getString(R.string.error_noevents));
                            errorScreen.setVisibility(View.VISIBLE);
                        }

                        progress.setVisibility(View.GONE);
                    }
                });
        suggestLogin();
    }

    /**
     * Método que sugere ao usuário fazer login
     */
    private void suggestLogin() {
        if(!preferences.contains("suggest_login")) {
            AlertDialog alertDialog = new AlertDialog.Builder(this)
                    .setTitle(getString(R.string.suggest_login_title).replace("{appname}", getString(R.string.app_name)))
                    .setMessage(getString(R.string.suggest_login_sub))
                    .setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
                        @Override
                        public void onClick(DialogInterface p1, int p2) {
                            Intent intent = new Intent(MainActivity.this, FragmentActivity.class);
                            intent.putExtra("fragment", 2);
                            intent.putExtra("title", getString(R.string.login_do));
                            intent.putExtra("url", Other.getLoginUrl(MainActivity.this));
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

    /**
     * Método que abre o evento selecionado, criando uma Activity no mobile, ou abrindo na coluna direita no tablet.
     * @param event Evento a ser aberto.
     */
    public void openEvent(Event event) {
        if (!Other.isTablet(this)) {
            Intent intent = new Intent(this, FragmentActivity.class);
            intent.putExtra("fragment", 1);
            intent.putExtra("event", event);
            startActivity(intent);
        } else {
            FragmentTransaction ft = getSupportFragmentManager().beginTransaction();
            Fragment eventFragment = new EventFragment();
            Bundle bundle = new Bundle();
            bundle.putSerializable("event", event);
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
                    AlertDialog alertDialog = new AlertDialog.Builder(this)
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

                    alertDialog.show();
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

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_refresh:
                getEvents();
                return true;
            case R.id.menu_checkin:
                if(preferences.contains("qr_code")) {
                    Intent checkin = new Intent(MainActivity.this, FragmentActivity.class);
                    checkin.putExtra("fragment", 6);
                    startActivity(checkin);
                } else if(preferences.contains("refresh_token")) {
                    // Caso o usuário esteja com uma versão antiga do app
                    Intent intent = new Intent(MainActivity.this, FragmentActivity.class);
                    intent.putExtra("fragment", 2);
                    intent.putExtra("title", getString(R.string.getting_qrcode));
                    intent.putExtra("url", Other.getLoginUrl(MainActivity.this));
                    intent.putExtra("islogin", true);
                    startActivity(intent);

                    Other.showToast(this, getString(R.string.getting_qrcode));
                } else {
                    AlertDialog alertDialog = new AlertDialog.Builder(this)
                            .setTitle(getString(R.string.checkin_need_login))
                            .setMessage(getString(R.string.rsvp_need_sub))
                            .setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
                                @Override
                                public void onClick(DialogInterface p1, int p2) {
                                    Intent intent = new Intent(MainActivity.this, FragmentActivity.class);
                                    intent.putExtra("fragment", 2);
                                    intent.putExtra("title", getString(R.string.login_do));
                                    intent.putExtra("url", Other.getLoginUrl(MainActivity.this));
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
                Other.openMeetupApp(this, "http://meetup.com/" + getString(R.string.meetup_id));
                return true;
            default:
                return
                        super.onOptionsItemSelected(item);
        }
    }
}
