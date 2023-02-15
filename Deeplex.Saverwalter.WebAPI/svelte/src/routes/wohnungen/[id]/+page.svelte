<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import Adresse from '../../../components/Adresse.svelte';
	import { request_options } from '../../../services/utils';
	import type { WohnungEntry } from '../../../types/wohnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<WohnungEntry> = fetch(
		`/api/wohnungen/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<h1>
	<Grid>
		{#await async}
			<Row>
				<TextInputSkeleton />
			</Row>
		{:then x}
			<Row>
				<TextInputSkeleton />
			</Row>
			<Adresse adresse={x.adresse} />
			<Row>
				<TextInput labelText="Bezeichnung" value={x.bezeichnung} />
				<TextInput labelText="Wohnfläche" value={x.wohnflaeche} />
				<TextInput labelText="Nutzfläche" value={x.nutzflaeche} />
				<TextInput labelText="Einheiten" value={x.einheiten} />
			</Row>
			<Row>
				<TextInput labelText="Notiz" value={x.notiz} />
			</Row>
		{/await}
	</Grid>
</h1>
