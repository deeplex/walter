<script lang="ts">
	import {
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { ErhaltungsaufwendungEntry } from '../../../../types/erhaltungsaufwendung.type';
	import { request_options } from '../../../../services/utils';
	import WalterDatePicker from '../../../../components/WalterDatePicker.svelte';

	export let data: PageData;
	const async: Promise<ErhaltungsaufwendungEntry> = fetch(
		`/api/erhaltungsaufwendungen/${data.id}`,
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
			<TextInputSkeleton />
		</Row>
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<TextInput labelText="Typ" value={x.bezeichnung} />
			<TextInput labelText="Aussteller" value={x.aussteller.name} />
		</Row>
		<Row>
			<TextInput labelText="Wohnung" value={x.wohnung.anschrift} />
			<WalterDatePicker value={x.datum} labelText="Datum" />
			<TextInput labelText="Betrag" value={x.betrag} />
		</Row>
		<Row>
			<TextInput labelText="Notiz" value={x.notiz} />
		</Row>
	{/await}
</Grid>
