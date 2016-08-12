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
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.widget.AppCompatEditText;
import android.support.v7.widget.SwitchCompat;
import android.view.View;
import android.support.v4.app.Fragment;
import android.text.InputFilter;
import android.view.LayoutInflater;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.LinearLayout;
import android.widget.LinearLayout.LayoutParams;
import com.google.gson.Gson;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import java.util.ArrayList;
import java.util.List;
import org.gdgsp.R;
import org.gdgsp.activity.MainActivity;
import org.gdgsp.model.Event;
import org.gdgsp.model.ObjectToSend;
import org.gdgsp.model.Question;
import org.gdgsp.other.Other;

/**
 * Fragment onde o usuário realiza RSVP.
 */
public class RSVPFragment extends Fragment {
	private Activity activity;
	private View view;
	private Event event;
	private SwitchCompat responseSwitch;
	private TextView responseText;
	private List<EditText> entries = new ArrayList<EditText>();

	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		this.activity = getActivity();
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		view = inflater.inflate(R.layout.fragment_rsvp, container, false);

		event = (Event)getActivity().getIntent().getSerializableExtra("event");
		responseSwitch = (SwitchCompat) view.findViewById(R.id.response_switch);

		responseText = (TextView)view.findViewById(R.id.response_text);

		final LinearLayout content = (LinearLayout)view.findViewById(R.id.content);

		final boolean checked = event.getResponse() != null && !event.getResponse().equals("no");
		final boolean isWaitlist = event.getYes_rsvp_count() == event.getRsvp_limit() && !event.getResponse().equals("yes");

		content.setVisibility(checked ? View.VISIBLE : View.GONE);

		responseSwitch.setChecked(checked);

		responseText.setText(checked ? isWaitlist ? getString(R.string.wait_list) : getString(R.string.yes) : getString(R.string.no));

		responseSwitch.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
			@Override
			public void onCheckedChanged(CompoundButton p1, boolean p2) {
				responseText.setText(p2 ? isWaitlist ? getString(R.string.wait_list) : getString(R.string.yes) : getString(R.string.no));

				content.setVisibility(p2 ? View.VISIBLE : View.GONE);
			}
		});

		LayoutParams params = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT);

		for(int i = 0; i < event.getSurvey_questions().size(); i++) {
			Question question = event.getSurvey_questions().get(i);

			TextView textView = new TextView(activity);
			textView.setText(question.getQuestion());

			EditText editText = new AppCompatEditText(activity);
			editText.setSingleLine(true);
			editText.setMaxLines(1);
			InputFilter[] FilterArray = new InputFilter[1];
			FilterArray[0] = new InputFilter.LengthFilter(250);
			editText.setFilters(FilterArray);

			if(event.getAnswers() != null) {
				editText.setText(event.getAnswers().get(i).getAnswer());
			}

			entries.add(editText);

			content.addView(textView, params);
			content.addView(editText, params);
		}

		final Button send = (Button)view.findViewById(R.id.button_send);

		send.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View p1) {
				ObjectToSend toSend = new ObjectToSend();
				toSend.setResponse(responseSwitch.isChecked() ? "yes" : "no");

				List<Question> list = new ArrayList<Question>();

				for(int i = 0; i < entries.size(); i++) {
					Question question = event.getSurvey_questions().get(i);
					question.setAnswer(entries.get(i).getText().toString());

					list.add(question);
				}

				toSend.setAnswers(list);

				Gson gson = new Gson();
				String json = gson.toJson(toSend);

				send.setEnabled(false);

				Ion.with(getContext())
						.load(Other.getRSVPUrl(activity, event.getId()))
						.setBodyParameter("json", json)
						.setBodyParameter("refresh_token", Other.getRefreshToken(activity))
						.asString()
						.setCallback(new FutureCallback<String>() {
							@Override
							public void onCompleted(Exception e, String response) {
								if(e != null) {
									send.setEnabled(true);
									Other.showToast(activity, getString(R.string.rsvp_error));
									return;
								}

								if(response.matches("waitlist|yes|no")) {
									String message = "";

									switch(response) {
										case "yes":
											message = getString(R.string.rsvp_success);
											break;
										case "waitlist":
											message = getString(R.string.rsvp_waitlist);
											break;
										case "no":
											message = getString(R.string.rsvp_no);
											break;
									}

									Other.showToast(activity, message);

									Intent intent = new Intent(activity, MainActivity.class);
									intent.putExtra("fromrsvp", true);
									MainActivity.openEvent = event.getId();
									intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
									startActivity(intent);
								} else {
									send.setEnabled(true);
									Other.showToast(activity, getString(R.string.rsvp_error));
								}
							}
						});
			}
		});
		return view;
	}
}
