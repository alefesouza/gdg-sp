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
import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import org.gdgsp.R;
import org.gdgsp.adapter.PeopleAdapter;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;

/**
 * Fragment onde é exibido a lista de pessoas que deram alguma resposta ao evento.
 */
public class PeopleFragment extends Fragment {
    private Activity activity;
    private View view;
	
	private List<Person> listPeople = new ArrayList<Person>();
    private RecyclerView list;
    private ProgressBar progress;
	private PeopleAdapter adapter;

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

        list = (RecyclerView)view.findViewById(R.id.list);
        progress = (ProgressBar)view.findViewById(R.id.progress);

		list.setVisibility(View.VISIBLE);
		progress.setVisibility(View.GONE);
		
		Gson gson = new Gson();

		Type datasetListType = new TypeToken<List<Person>>() {}.getType();
		listPeople = gson.fromJson(getArguments().getString("peopleJson"), datasetListType);

        adapter = new PeopleAdapter(activity, listPeople);
        list.setLayoutManager(new LinearLayoutManager(activity));
        list.setAdapter(adapter);
		
		return view;
    }

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		if(getArguments().getInt("position") == 0) {
			inflater = getActivity().getMenuInflater();
			inflater.inflate(R.menu.people, menu);
		}

		super.onCreateOptionsMenu(menu, inflater);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
            case R.id.menu_random:
				// Se o tamanho da lista for maior que 0 significa que já carregou
				if(listPeople.size() > 0) {
					// Aqui vai gerar um número aleatório entre 0 e o tamanho da lista - 1
					Random random = new Random();
					int number = random.nextInt(listPeople.size());
					// Após isso o aplicativo vai dar scroll e exibir a pessoa na posição do número aleatório
					((LinearLayoutManager)list.getLayoutManager()).scrollToPositionWithOffset(number, 0);
					Other.showMessage(activity, getString(R.string.raffle_person), listPeople.get(number).getName());
				}
				return true;
            default:
                return
					super.onOptionsItemSelected(item);
        }
	}
}
