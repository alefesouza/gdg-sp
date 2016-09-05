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
import android.os.Bundle;
import android.support.design.widget.TabLayout;
import android.support.v4.view.ViewPager;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.MenuItem;
import android.view.View;
import android.widget.ProgressBar;
import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;
import org.gdgsp.R;
import org.gdgsp.adapter.TabAdapter;
import org.gdgsp.fragment.PeopleFragment;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;
import org.gdgsp.model.*;

/**
 * Activity onde é exibida as pessoas que deram alguma resposta ao evento, pode ter até três PeopleFragment.
 */
public class PeopleActivity extends AppCompatActivity {
	private Toolbar toolbar;
	private TabLayout tabLayout;
	private ViewPager viewPager;
	private ProgressBar progress;
	private TabAdapter adapter;
	private List<Person> listPeople;
	private String jsonPeople = null;
	private Event event;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_people);

		event = (Event)getIntent().getSerializableExtra("event");
		
		toolbar = (Toolbar) findViewById(R.id.toolbar);
		setSupportActionBar(toolbar);

		getSupportActionBar().setDisplayHomeAsUpEnabled(true);

		getSupportActionBar().setTitle(event.getWho());

		viewPager = (ViewPager) findViewById(R.id.viewpager);
		viewPager.setOffscreenPageLimit(3);
		adapter = new TabAdapter(getSupportFragmentManager());
		viewPager.setAdapter(adapter);

		tabLayout = (TabLayout) findViewById(R.id.sliding_tabs);
		tabLayout.setupWithViewPager(viewPager);

		progress = (ProgressBar)findViewById(R.id.progress);

		if(savedInstanceState == null || savedInstanceState.getString("peopleJson") == null) {
			getPeople();
		} else {
			progress.setVisibility(View.GONE);
			tabLayout.setVisibility(View.VISIBLE);

			setupViewPager(savedInstanceState.getString("peopleJson"));
		}
	}

	public void setupViewPager(String json) {
		Gson gson = new Gson();

		Type datasetListType = new TypeToken<List<Person>>() {}.getType();
		jsonPeople = json;
		listPeople = gson.fromJson(json, datasetListType);

		progress.setVisibility(View.GONE);
		tabLayout.setVisibility(View.VISIBLE);

		List<Person> listGo = new ArrayList<Person>(), listWait = new ArrayList<Person>(), listDontGo = new ArrayList<Person>();

		// "There is nothing like LINQ for Java"
		for(Person p : listPeople) {
			switch(p.getResponse()) {
				case "yes":
					listGo.add(p);
					break;
				case "waitlist": case "watching":
					listWait.add(p);
					break;
				case "no":
					listDontGo.add(p);
					break;
			}
		}

		String[] titles = { getString(R.string.people_go), getString(R.string.people_wait), getString(R.string.people_dont_go)};

		for(int i = 0; i < titles.length; i++) {
			List<Person> list;

			if(i == 0) list = listGo; else if(i == 1) list = listWait; else list = listDontGo;

			if(list.size() > 0) {
				Bundle bundle = new Bundle();
				bundle.putInt("position", i);
				bundle.putString("peopleJson", gson.toJson(list));
				PeopleFragment fragment = new PeopleFragment();
				fragment.setArguments(bundle);
				adapter.addFragment(fragment, titles[i]);
			}
		}

		adapter.notifyDataSetChanged();
	}

	/**
	 * Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
	 */
	private void getPeople() {
		Ion.with (this)
				.load(Other.getRSVPSUrl(this, event.getId()))
				.setBodyParameter("refresh_token", Other.getRefreshToken(this))
				.asJsonArray()
				.setCallback(new FutureCallback<JsonArray>() {
					@Override
					public void onCompleted(Exception e, final JsonArray json) {
						if(e != null) {
							AlertDialog alertDialog = new AlertDialog.Builder(PeopleActivity.this)
									.setTitle(getString(R.string.connection_error))
									.setMessage(getString(R.string.try_again))
									.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
										@Override
										public void onClick(DialogInterface p1, int p2) {
											getPeople();
										}
									})
									.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
										@Override
										public void onClick(DialogInterface p1, int p2) {
											finish();
										}
									})
									.create();

							alertDialog.show();
							e.printStackTrace();
							return;
						}

						setupViewPager(json.toString());
					}
				});
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
			case android.R.id.home:
				finish();
				return true;
			default:
				return
						super.onOptionsItemSelected(item);
		}
	}

	@Override
	protected void onSaveInstanceState(Bundle outState) {
		outState.putString("peopleJson", jsonPeople);
		super.onSaveInstanceState(outState);
	}
}
