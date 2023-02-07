<script lang="ts">
	import { Grid, Row, TextInput, TextInputSkeleton } from 'carbon-components-svelte';
	import Adresse from '../../../components/adresse.svelte';
	import { WohnungEntry } from './classes';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const async = fetch(`/api/wohnungen/${window.location.href.split('/').at(-1)}`, request_options)
		.then((e) => e.json())
		.then((e) => new WohnungEntry(e));
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
