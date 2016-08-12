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
import android.support.v4.app.Fragment;
import android.support.v7.app.AppCompatActivity;
import android.text.Html;
import android.text.method.LinkMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;

import org.gdgsp.R;
import org.gdgsp.activity.MainActivity;
import org.gdgsp.model.Event;
import org.gdgsp.other.Other;

import java.util.List;

public class SendNotificationFragment extends Fragment {
    private AppCompatActivity activity;
    private View view;

    private CheckBox checkBox;
    private Spinner spinner;
    private EditText title, link, image, message;
    private Button send;
    private int selectedPosition = 0;

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
        view = inflater.inflate(R.layout.fragment_notification, container, false);

        checkBox = (CheckBox)view.findViewById(R.id.checkbox_to_all);
        spinner = (Spinner)view.findViewById(R.id.spinner_events);

        title = (EditText)view.findViewById(R.id.edit_title);
        link = (EditText)view.findViewById(R.id.edit_link);
        image = (EditText)view.findViewById(R.id.edit_image);
        message = (EditText)view.findViewById(R.id.edit_message);

        send = (Button)view.findViewById(R.id.button_send);

        final List<Event> listEvents = MainActivity.listEvents;

        if(listEvents.size() > 0) {
            String[] events = new String[listEvents.size()];

            for (int i = 0; i < listEvents.size(); i++) {
                events[i] = listEvents.get(i).getName();
            }

            checkBox.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton compoundButton, boolean b) {
                    spinner.setVisibility(b ? View.GONE : View.VISIBLE);
                }
            });

            ArrayAdapter<String> adapter = new ArrayAdapter<String>(activity, android.R.layout.simple_spinner_dropdown_item, events);
            spinner.setAdapter(adapter);
            spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                public void onItemSelected(AdapterView<?> parent, View v, int position, long id) {
                    selectedPosition = position;
                }

                @Override
                public void onNothingSelected(AdapterView<?> adapterView) {

                }
            });

            spinner.setSelection(0);
        } else {
            checkBox.setVisibility(View.GONE);
        }

        send.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                send.setEnabled(false);

                Ion.with(getContext())
                        .load(Other.getNotificationUrl(activity))
                        .setBodyParameter("app_key", Other.getAppKey())
                        .setBodyParameter("title", title.getText().toString())
                        .setBodyParameter("link", link.getText().toString())
                        .setBodyParameter("image", image.getText().toString())
                        .setBodyParameter("message", message.getText().toString())
                        .setBodyParameter("eventid", checkBox.isChecked() ? "" : String.valueOf(listEvents.get(selectedPosition).getId()))
                        .setBodyParameter("refresh_token", Other.getRefreshToken(activity))
                        .asString()
                        .setCallback(new FutureCallback<String>() {
                            @Override
                            public void onCompleted(Exception e, String response) {
                                if(e != null) {
                                    send.setEnabled(true);
                                    Other.showToast(activity, getString(R.string.connection_error));
                                    return;
                                }

                                if(response.matches("notification_send|invalid_user|try_again")) {
                                    String message = "";

                                    switch(response) {
                                        case "notification_send":
                                            message = getString(R.string.notification_send);
                                            break;
                                        case "invalid_user":
                                            message = getString(R.string.notification_invalid_user);
                                            break;
                                        case "invalid_key":
                                            message = getString(R.string.invalid_key);
                                            break;
                                        case "try_again":
                                            message = getString(R.string.notification_try_again);
                                            break;
                                    }

                                    activity.finish();

                                    Other.showToast(activity, message);
                                }
                            }
                        });
            }
        });

        return view;
    }
}
