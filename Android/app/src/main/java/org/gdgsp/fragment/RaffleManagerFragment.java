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
import android.os.Bundle;
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
import android.widget.Toast;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import com.koushikdutta.async.future.FutureCallback;
import com.koushikdutta.ion.Ion;
import java.lang.reflect.Type;
import java.util.List;
import org.gdgsp.R;
import org.gdgsp.adapter.RaffleManagerAdapter;
import org.gdgsp.model.Raffle;
import org.gdgsp.other.Other;

/**
 * Fragment onde é exibido as pessoas que sortearam a si mesmo no evento, exibida apenas para organizadores e usuários permitidos.
 */
public class RaffleManagerFragment extends Fragment {
    private Activity activity;
    private View view;

    private Gson gson;
    private List<Raffle> listPeople;
    private RecyclerView list;
    private ProgressBar progress;

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
        view = inflater.inflate(R.layout.content_main, container, false);

        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

		gson = new Gson();

		getPeople("");

        return view;
    }

	/**
	 * Método que solicita a lista de pessoas sorteadas.
	 */
	private void getPeople(String empty) {
		progress.setVisibility(View.VISIBLE);
		list.setVisibility(View.GONE);

		Ion.with (this)
			.load(Other.getRaffleUrl(activity, activity.getIntent().getIntExtra("eventid", 0)))
			.setBodyParameter("app_key", Other.getAppKey())
			.setBodyParameter("manager","true")
			.setBodyParameter("empty", empty)
			.setBodyParameter("refresh_token", Other.getRefreshToken(activity))
			.asString()
			.setCallback(new FutureCallback<String>() {
				@Override
				public void onCompleted(Exception e, String result) {
					if(e != null) {
						AlertDialog alertDialog = new AlertDialog.Builder(activity)
							.setTitle(getString(R.string.connection_error))
							.setMessage(getString(R.string.try_again))
							.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface p1, int p2) {
									getPeople("false");
								}
							})
							.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface p1, int p2) {
									activity.finish();
								}
							})
							.create();

						alertDialog.setCanceledOnTouchOutside(false);
						alertDialog.show();
						e.printStackTrace();
						return;
					}

					switch(result) {
						case "invalid_user": case "invalid_key":
							Toast.makeText(activity, "Usuário ou chave do aplicativo inválida", Toast.LENGTH_SHORT).show();
							activity.finish();
							break;
						case "[]":
							AlertDialog alertDialog = new AlertDialog.Builder(activity)
								.setTitle("Não há pessoas sorteadas nesse evento")
								.setMessage(getString(R.string.login_error_sub))
								.setPositiveButton(getString(R.string.yes), new DialogInterface.OnClickListener() {
									@Override
									public void onClick(DialogInterface p1, int p2) {
										getPeople("false");
									}
								})
								.setNegativeButton(getString(R.string.no), new DialogInterface.OnClickListener() {
									@Override
									public void onClick(DialogInterface p1, int p2) {
										activity.finish();
									}
								})
								.create();

							alertDialog.setCanceledOnTouchOutside(false);
							alertDialog.show();
							break;
						default:
							Type datasetListType = new TypeToken<List<Raffle>>() {}.getType();
							listPeople = gson.fromJson(result, datasetListType);

							RaffleManagerAdapter adapter = new RaffleManagerAdapter(activity, listPeople);
							list.setLayoutManager(new LinearLayoutManager(activity));
							list.setAdapter(adapter);

							progress.setVisibility(View.GONE);
							list.setVisibility(View.VISIBLE);
					}
				}
			});
	}

    @Override
    public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
        inflater = getActivity().getMenuInflater();
        inflater.inflate(R.menu.menu_raffle, menu);

        super.onCreateOptionsMenu(menu, inflater);
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_refresh:
				getPeople("false");
                return true;
            case R.id.menu_empty:
				getPeople("true");
                return true;
            default:
                return
					super.onOptionsItemSelected(item);
        }
    }
}
