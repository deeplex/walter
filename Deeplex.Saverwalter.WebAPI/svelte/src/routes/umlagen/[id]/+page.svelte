<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import { convertDate, request_options } from '../../../services/utils';
	import type { UmlageEntry } from '../../../types/umlage.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<UmlageEntry> = fetch(
		`/api/umlagen/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
			<TextInputSkeleton />
		</Row>
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<TextInput labelText="Bezeichnung" value={x.typ} />
			<TextInput labelText="Wohnfläche" value={x.wohnungenBezeichnung} />
		</Row>
		<Row>
			<TextInput labelText="Notiz" value={x.notiz} />
		</Row>
	{/await}
</Grid>
