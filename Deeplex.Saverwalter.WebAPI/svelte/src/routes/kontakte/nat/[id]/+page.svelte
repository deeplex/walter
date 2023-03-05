<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import type { WalterNatuerlichePersonEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import {
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterKontakte,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail
	} from '$WalterComponents';

	export let data: PageData;
	const url = `/api/kontakte/nat/${data.id}`;

	const a: WalterNatuerlichePersonEntry = data.a;
</script>

<WalterHeaderDetail {a} {url} title={a.name} />

<WalterGrid>
	<Row>
		<WalterTextInput bind:value={a.vorname} labelText="Vorname" />
		<WalterTextInput bind:value={a.nachname} labelText="Nachname" />
	</Row>
	<WalterPerson value={a} />

	<Accordion>
		<!-- TODO add here -->
		<WalterKontakte title="Juristische Personen" rows={a.juristischePersonen} />
		{#await a then}
			<WalterWohnungen title="Wohnungen" rows={a.wohnungen} />
			<WalterVertraege title="Verträge" rows={a.vertraege} />
		{/await}
	</Accordion>
</WalterGrid>
