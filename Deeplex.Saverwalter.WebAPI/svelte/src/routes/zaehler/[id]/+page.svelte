<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import Adresse from '../../../components/Adresse.svelte';
	import { request_options } from '../../../services/utils';
	import type { ZaehlerEntry } from '../../../types/zaehler.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<ZaehlerEntry> = fetch(
		`/api/zaehler/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<TextInput required labelText="Kennnummer" value={x.kennnummer} />
			<TextInput required labelText="Typ" value={x.typ} />
		</Row>
		<Adresse adresse={x.adresse} />
	{/await}
</Grid>
