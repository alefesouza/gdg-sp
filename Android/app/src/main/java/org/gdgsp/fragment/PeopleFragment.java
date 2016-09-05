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
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v4.app.Fragment;
import android.support.v7.app.AlertDialog;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;

import java.lang.reflect.Type;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Random;
import java.util.Timer;
import java.util.TimerTask;

import org.gdgsp.R;
import org.gdgsp.adapter.PeopleAdapter;
import org.gdgsp.model.Event;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;

/**
 * Fragment onde é exibido a lista de pessoas que deram alguma resposta ao evento.
 * Esses código tem algumas gambiarras pra arrumar os save state ao escolher somente quem tem o aplicativo.
 */
public class PeopleFragment extends Fragment {
    private Activity activity;
    private View view;

    private Gson gson;
    private List<Person> listPeople;
    private List<Person> hasAppPeople = new ArrayList<Person>();
    private List<Person> selectedList = new ArrayList<Person>();
    private RecyclerView list;
    private ProgressBar progress;
    private PeopleAdapter adapter;
	private Person person;
	
	private int count;

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        this.activity = getActivity();
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        setHasOptionsMenu(true);
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        view = inflater.inflate(R.layout.fragment_people, container, false);

		SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(activity);
		
        gson = new Gson();
		if(preferences.contains("member_profile")) {
            Type datasetListType = new TypeToken<Person>() {}.getType();
            person = gson.fromJson(preferences.getString("member_profile", ""), datasetListType);
		}

        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

        list.setVisibility(View.VISIBLE);
        progress.setVisibility(View.GONE);

        Type datasetListType = new TypeToken<List<Person>>() {}.getType();
        listPeople = gson.fromJson(getArguments().getString("peopleJson"), datasetListType);

        if(getArguments().getInt("position") == 0) {
            for (Person p : listPeople) {
                if (p.isHas_app()) {
                    hasAppPeople.add(p);
                }
            }
        }

        selectedList.clear();
        selectedList.addAll(listPeople);

        adapter = new PeopleAdapter(activity, selectedList);
        list.setLayoutManager(new LinearLayoutManager(activity));
        list.setAdapter(adapter);

        return view;
    }

    @Override
    public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
        if(getArguments().getInt("position") == 0) {
            inflater = getActivity().getMenuInflater();
            inflater.inflate(R.menu.menu_people, menu);
        }

        super.onCreateOptionsMenu(menu, inflater);
    }

    @Override
    public void onPrepareOptionsMenu(Menu menu) {
        if(selectedList.size() == hasAppPeople.size()) {
            menu.findItem(R.id.menu_hasapp).setChecked(true);
        }
        super.onPrepareOptionsMenu(menu);
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_random:
                // Se o tamanho da lista for maior que 0 significa que já carregou
                if(selectedList.size() > 0) {
                    // Aqui vai gerar um número aleatório entre 0 e o tamanho da lista - 1
                    Random random = new Random();
                    int number = random.nextInt(selectedList.size());
                    // Após isso o aplicativo vai dar scroll e exibir a pessoa na posição do número aleatório
                    ((LinearLayoutManager)list.getLayoutManager()).scrollToPositionWithOffset(number, 0);
					
					if(person != null && selectedList.get(number).getId() == person.getId()) {
						count = 0;
						
						// Verifica o tanto de tempo que a pessoa ficou com o alerta aberto, se demorar muito vai ter coisa errada, como mudar a hora do sistema e aguardar o sorteio
						final Timer timer = new Timer();
						timer.scheduleAtFixedRate(new TimerTask() {
								@Override
								public void run() {
									count++;                
								}
							}, 1000, 1000);
						
						SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss dd/MM/yyyy");
						String localDate = sdf.format(new Date());
						SimpleDateFormat dbDateSDF = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
						final String dbDate = dbDateSDF.format(new Date());
						
						AlertDialog alertDialog = new AlertDialog.Builder(activity)
							.setTitle(getString(R.string.raffle_person))
							.setMessage("Você!\n\nSorteado às " + localDate)
							.setPositiveButton(getString(R.string.send), new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface p1, int p2) {
									timer.cancel();

                                    sendRaffle(dbDate);
								}
							})
							.setNegativeButton("Cancelar", new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface p1, int p2) {
									timer.cancel();
								}
							})

							.create();

						alertDialog.setCanceledOnTouchOutside(false);
						alertDialog.show();
					} else {
						AlertDialog alertDialog = new AlertDialog.Builder(activity)
							.setTitle(getString(R.string.raffle_person))
							.setMessage(selectedList.get(number).getName())
							.setPositiveButton("OK", null)
							.create();

						alertDialog.setCanceledOnTouchOutside(false);
						alertDialog.show();
					}
                }
                return true;
            case R.id.menu_hasapp:
                // Exibir apenas pessoas que usam o aplicativo, essa informação vem de um banco de dados no back-end
                item.setChecked(!item.isChecked());

                selectedList.clear();

                selectedList.addAll(item.isChecked() ? hasAppPeople : listPeople);

                adapter.notifyDataSetChanged();
                return true;
            default:
                return
					super.onOptionsItemSelected(item);
        }
    }

	public void sendRaffle(final String dbDate) {
		final ProgressDialog progressDialog = ProgressDialog.show(activity, getString(R.string.app_name), getString(R.string.sending), false, false);
		progressDialog.show();

		Event event = (Event)activity.getIntent().getSerializableExtra("event");

		Ion.with(getContext())
				.load(Other.getRaffleUrl(activity, event.getId()))
				.setBodyParameter("app_key", Other.getAppKey())
				.setBodyParameter("raffle_date", dbDate)
				.setBodyParameter("seconds", String.valueOf(count))
				.setBodyParameter("refresh_token", Other.getRefreshToken(activity))
				.asString()
				.setCallback(new FutureCallback<String>() {
					@Override
					public void onCompleted(Exception e, String response) {
						progressDialog.dismiss();

						if(e != null) {
                            AlertDialog alertDialog = new AlertDialog.Builder(activity)
                                    .setTitle(getString(R.string.connection_error))
                                    .setMessage(getString(R.string.login_error_sub))
                                    .setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
                                        @Override
                                        public void onClick(DialogInterface p1, int p2) {
                                            sendRaffle(dbDate);
                                        }
                                    })
                                    .setNegativeButton(getString(R.string.no), null)

                                    .create();

                            alertDialog.setCanceledOnTouchOutside(false);
                            alertDialog.show();
							e.printStackTrace();
							return;
						}

						if(response.matches("success|invalid_user|invalid_key")) {
							String message = "";

							switch(response) {
								case "success":
									message = getString(R.string.raffle_success);
									break;
								case "invalid_user":
									message = getString(R.string.notification_invalid_user);
									break;
								case "invalid_key":
									message = getString(R.string.invalid_key);
									break;
							}

							Other.showToast(activity, message);
						}
					}
				});
	}
}
