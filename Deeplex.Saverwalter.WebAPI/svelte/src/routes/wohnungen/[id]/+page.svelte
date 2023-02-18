<script lang="ts">
	import {
		Column,
		ComboBox,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import Adresse from '../../../components/Adresse.svelte';
	import WalterComboBox from '../../../components/WalterComboBox.svelte';
	import { request_options } from '../../../services/utils';
	import type { KontaktListEntry } from '../../../types/kontaktlist.type';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<WohnungEntry> = fetch(
		`/api/wohnungen/${data.id}`,
		request_options
	).then((e) => e.json());

	// TODO: See how many data is just not used... Maybe an extra controller?
	const kontakte: Promise<ComboBoxItem[]> = fetch(
		`/api/kontakte`,
		request_options
	)
		.then((e) => e.json())
		.then((e) =>
			e.map(
				(f: KontaktListEntry) => ({ id: f.guid, text: f.name } as ComboBoxItem)
			)
		);
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<WalterComboBox
				titleText="Aussteller"
				items={kontakte}
				selectedId={x.besitzerId}
			/>
		</Row>
		<Adresse adresse={x.adresse} />
		<Row>
			<TextInput labelText="Besitzer" value={x.bezeichnung} />
			<TextInput labelText="Wohnfläche" value={x.wohnflaeche} />
			<TextInput labelText="Nutzfläche" value={x.nutzflaeche} />
			<TextInput labelText="Einheiten" value={x.einheiten} />
		</Row>
		<Row>
			<TextInput labelText="Notiz" value={x.notiz} />
		</Row>
	{/await}
</Grid>
