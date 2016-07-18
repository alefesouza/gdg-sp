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
import android.content.SharedPreferences.Editor;
import android.os.Bundle;
import android.os.Handler;
import android.preference.Preference;
import android.preference.PreferenceFragment;
import android.preference.PreferenceManager;
import com.onesignal.OneSignal;
import org.gdgsp.R;

/**
 * PreferenceFragment das configurações do aplicativo.
 */
public class SettingsFragment extends PreferenceFragment {
	private Activity activity;
	private SharedPreferences preferences;

	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = getActivity();
	}

	@SuppressWarnings("deprecation")
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		addPreferencesFromResource(R.xml.settings);
		
		preferences = PreferenceManager.getDefaultSharedPreferences(getActivity());

		Preference prefNotification = findPreference("prefNotification");
		prefNotification.setOnPreferenceChangeListener(new Preference.OnPreferenceChangeListener() {
			@Override
			public boolean onPreferenceChange(Preference preference, final Object newValue) {
				// É necessário um Handler de pelo menos uns 100 milissegundos para o Android identificar a mudança.
				new Handler().postDelayed(new Runnable() {
					@Override
					public void run() {
						OneSignal.setSubscription((boolean)newValue);
					}
				}, 100);
				return true;
			}
		});
	}
}
