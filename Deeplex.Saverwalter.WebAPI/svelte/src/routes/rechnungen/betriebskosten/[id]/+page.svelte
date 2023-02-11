<script lang="ts">
	import { Grid, Row, TextInput, TextInputSkeleton } from 'carbon-components-svelte';
	import type { BetriebskostenrechnungEntry } from '../../../../types/betriebskostenrechnung.type';
	import type { PageData, RouteParams } from './$types';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	export let data: PageData;
	const async: Promise<BetriebskostenrechnungEntry> = fetch(
		`/api/betriebskostenrechnungen/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<Grid>
	{#await async}
		<TextInputSkeleton />
	{:then x}
		<Row><TextInput value={x.umlage} /></Row>
	{/await}
</Grid>
