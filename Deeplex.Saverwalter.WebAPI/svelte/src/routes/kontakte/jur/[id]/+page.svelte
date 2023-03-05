<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import {
		WalterKontakte,
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail
	} from '$WalterComponents';

	export let data: PageData;

	let a = data.a;
</script>

<WalterHeaderDetail {a} url={data.url} title={a.name} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.name} />
	</Row>
	<WalterPerson value={a} />

	<Accordion>
		<!-- TODO add -->
		<WalterKontakte title="Mitglieder" rows={a.mitglieder} />
		<!-- TODO add -->
		<WalterKontakte title="Juristische Personen" rows={a.juristischePersonen} />
		{#await a then}
			<WalterWohnungen title="Wohnungen" rows={a.wohnungen} />
			<WalterVertraege title="Verträge" rows={a.vertraege} />
		{/await}
	</Accordion>
</WalterGrid>
