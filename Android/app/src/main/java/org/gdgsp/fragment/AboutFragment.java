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
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.support.v7.app.AppCompatActivity;
import android.text.Html;
import android.text.method.LinkMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import org.gdgsp.R;
import org.gdgsp.other.Other;

/**
 * Fragment da tela "Sobre".
 */
public class AboutFragment extends Fragment {
	private AppCompatActivity activity;
	private View view;

	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = (AppCompatActivity)getActivity();
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, 	Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.fragment_about, container, false);

		String about_text = "<h3>GDG</h3>" +
				"<p>O Google Developers Group é uma iniciativa de pessoas interessadas em construir com tecnologia e disseminar o conhecimento. Nossos eventos são direcionados para a comunidade de desenvolvedores, engenheiros, designers e empreendedores, organizados pelos nossos membros de forma voluntária e sem fins lucrativos. Encontre outros capítulos do GDG no Brasil no <a href=\"https://developers.google.com/groups/directory/Brazil\">Google Developers</a>.</p>" +
				"<p><h3>Aplicativo</h3></p>" +
				"<p>" + activity.getString(R.string.app_name) + " para Android versão: " + Other.getAppVersion(activity) + "</p>" +
				"<p>Aplicativo desenvolvido por <a href=\"http://alefesouza.com\">Alefe Souza</a></p>" +
				"<p>Esse aplicativo foi desenvolvido em código aberto para Android, Universal Windows Platform e Xamarin.Forms, você pode ver o código exato dos aplicativos e o back-end em PHP no meu <a href=\"http://github.com/alefesouza/gdg-sp\">GitHub</a>, procurei deixar o código para ser facilmente adaptado para outros meetups, deixando informações de como fazer isso em cada projeto.</p>" +
				"Nesse aplicativo foi utilizado:" +
				"<br><br><a href=\"http://icons8.com\">Icons8</a>" +
				"<br><a href=\"http://github.com/koush/ion\">Ion</a>" +
				"<br><a href=\"http://onesignal.com\">OneSignal</a>" +
				"<br><a href=\"http://github.com/vinc3m1/RoundedImageView\">Rounded Image View</a>" +
				"<br><a href=\"http://github.com/google/gson\">Gson</a>" +
				"<br><a href=\"http://developer.android.com/topic/libraries/support-library/index.html\">Android Support Libraries</a>" +
				"<br><a href=\"http://developers.google.com/android/guides/overview\">Google Play Services</a>";

		TextView about = (TextView)view.findViewById(R.id.about);
		about.setMovementMethod(LinkMovementMethod.getInstance());
		about.setText(Html.fromHtml(about_text));

		return view;
	}
}
