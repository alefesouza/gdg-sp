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
import org.gdgsp.model.*;
import android.text.*;

/**
 * Adapter dos cards de pessoas exibidos no PeopleFragment.
 */
public class RaffleManagerAdapter extends RecyclerView.Adapter <RecyclerView.ViewHolder> {
    private Context context;
    private List<Raffle> people;

    public RaffleManagerAdapter(Context context, List <Raffle> people) {
        this.context = context;
        this.people = people;
    }

    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, 	int viewType) {
        View item = LayoutInflater.from(parent.getContext())
			.inflate(R.layout.card_raffle, null);

        ItemViewHolder viewHolder = new ItemViewHolder(item);
        return viewHolder;
    }

    public void remove(int position) {
        people.remove(position);
        notifyItemRemoved(position);
    }

    public void add(Raffle card, int position) {
        people.add(card);
        notifyItemInserted(position);
    }

    @Override
    public void onBindViewHolder(final RecyclerView.ViewHolder holderr, final int position) {
        if (holderr instanceof ItemViewHolder) {
            ItemViewHolder holder = (ItemViewHolder)holderr;

            String name = people.get(position).getName();
            String raffle_date = people.get(position).getRaffle_date();
            String post_date = people.get(position).getPost_date();
            String image = people.get(position).getPhoto();
			String seconds = people.get(position).getSeconds();

            holder.name.setText(name);
			holder.raffle_date.setText(Html.fromHtml("<b>Sorteado em:</b> " + raffle_date));
			holder.post_date.setText(Html.fromHtml("<b>Recebido em:</b> " + post_date));
			holder.seconds.setText(Html.fromHtml("<b>Segundos com alerta aberto:</b> " + seconds));

            Ion.with (context)
				.load(image)
				.withBitmap()
				.intoImageView(holder.image);
        }
    }

    public class ItemViewHolder extends RecyclerView.ViewHolder {
        public TextView name, raffle_date, post_date,seconds;
        public ImageView image;
        public RelativeLayout content;

        public ItemViewHolder(View item) {
            super(item);
            name = (TextView)item.findViewById(R.id.name);
            raffle_date = (TextView)item.findViewById(R.id.raffle_date);
            post_date = (TextView)item.findViewById(R.id.post_date);
            seconds = (TextView)item.findViewById(R.id.seconds);
            image = (ImageView)item.findViewById(R.id.profileImage);
            content = (RelativeLayout)item.findViewById(R.id.content);
        }
    }

    @Override
    public int getItemCount() {
        if(people != null) {
            return people.size();
        } else {
            return 0;
        }
    }
}
