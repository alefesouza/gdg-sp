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

import android.os.Bundle;
import android.preference.PreferenceFragment;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentTransaction;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.KeyEvent;
import android.view.MenuItem;
import org.gdgsp.R;
import org.gdgsp.fragment.*;

/**
 * Activity que "hospeda" a maioria dos Fragments.
 */
public class FragmentActivity extends AppCompatActivity {
	private Toolbar toolbar;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		setContentView(R.layout.activity_fragment);

		toolbar = (Toolbar)findViewById(R.id.toolbar);
		setSupportActionBar(toolbar);

		getSupportActionBar().setHomeButtonEnabled(true);
		getSupportActionBar().setDisplayHomeAsUpEnabled(true);

		Fragment fragment = null;
		String title = null;

		switch(getIntent().getIntExtra("fragment", 0)) {
			case 0:
				title = getString(R.string.settings);

				PreferenceFragment settings = new SettingsFragment();
				getFragmentManager().beginTransaction().replace(R.id.fragment, settings).commit();
				break;
			case 1:
				title = "";
				fragment = new EventFragment();

				Bundle bundleEvent = new Bundle();
				bundleEvent.putSerializable("event", getIntent().getSerializableExtra("event"));

				fragment.setArguments(bundleEvent);
				break;
			case 2:
				title = getIntent().getStringExtra("title");
				fragment = new WebViewFragment();

				Bundle bundle = new Bundle();
				bundle.putString("url", getIntent().getStringExtra("url"));
				bundle.putBoolean("islogin", getIntent().getBooleanExtra("islogin", false));

				fragment.setArguments(bundle);
				break;
			case 3:
				title = getString(R.string.about);
				fragment = new AboutFragment();
				break;
			case 4:
				title = getString(R.string.rsvp);
				fragment = new RSVPFragment();
				break;
			case 5:
				title = getString(R.string.send_notification);
				fragment = new SendNotificationFragment();
				break;
			case 6:
				title = getString(R.string.do_checkin);
				fragment = new CheckinFragment();
				break;
		}

		if(fragment != null) {
			getSupportActionBar().setTitle(title);
			FragmentTransaction ft = getSupportFragmentManager().beginTransaction();
			ft.replace(R.id.fragment, fragment);
			ft.commit();
		}
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
	public boolean onKeyDown(int keyCode, KeyEvent event) {
		if (getIntent().getIntExtra("fragment", 0) != 2) {
			finish();
		}
		return super.onKeyDown(keyCode, event);
	}
}
