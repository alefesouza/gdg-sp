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

package org.gdgsp.adapter;

import android.content.Context;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import com.koushikdutta.ion.Ion;
import org.gdgsp.R;
import org.gdgsp.model.Person;
import org.gdgsp.other.Other;
import java.util.List;

/**
 * Adapter dos cards de pessoas exibidos no PeopleFragment.
 */
public class PeopleAdapter extends RecyclerView.Adapter <RecyclerView.ViewHolder> {
    private Context context;
    private List<Person> people;

    public PeopleAdapter(Context context, List <Person> people) {
        this.context = context;
        this.people = people;
    }

    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, 	int viewType) {
        View item = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.card_person, null);

        ItemViewHolder viewHolder = new ItemViewHolder(item);
        return viewHolder;
    }

    public void remove(int position) {
        people.remove(position);
        notifyItemRemoved(position);
    }

    public void add(Person card, int position) {
        people.add(card);
        notifyItemInserted(position);
    }

    @Override
    public void onBindViewHolder(final RecyclerView.ViewHolder holderr, final int position) {
        if (holderr instanceof ItemViewHolder) {
            ItemViewHolder holder = (ItemViewHolder)holderr;

            final String name = people.get(position).getName();
            final String intro = people.get(position).getIntro();
            String image = people.get(position).getPhoto();

            holder.name.setText(name);

			// Se colocar margin top direto vai ficar um padding bottom desnecessário em cards sem texto
			holder.intro.setText(intro.equals("") ? "" : "\n" + intro);

            Ion.with (context)
                    .load(image)
                    .withBitmap()
                    .intoImageView(holder.image);

            holder.content.setOnClickListener(new OnClickListener() {
                public void onClick(View v) {
					Other.openMeetupApp(context, "http://meetup.com/" + context.getString(R.string.meetup_id) + "/members/" + people.get(position).getId());
                }
            });
        }
    }

    public class ItemViewHolder extends RecyclerView.ViewHolder {
        public TextView name;
        public TextView intro;
        public ImageView image;
        public RelativeLayout content;

        public ItemViewHolder(View item) {
            super(item);
            name = (TextView)item.findViewById(R.id.name);
            intro = (TextView)item.findViewById(R.id.intro);
            image = (ImageView)item.findViewById(R.id.profileImage);
            content = (RelativeLayout)item.findViewById(R.id.content);
        }
    }

    @Override
    public int getItemCount() {
        return people.size();
    }
}
