<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { WalterNatuerlichePersonEntry } from '$WalterTypes';
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
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
	title={data.a.name}
/>

<WalterGrid>
	<Row>
		<WalterTextInput bind:value={data.a.vorname} labelText="Vorname" />
		<WalterTextInput bind:value={data.a.nachname} labelText="Nachname" />
	</Row>
	<WalterPerson value={data.a} />

	<Accordion>
		<!-- TODO add here -->
		<WalterKontakte
			title="Juristische Personen"
			rows={data.a.juristischePersonen}
		/>
		{#await data.a then}
			<WalterWohnungen
				kontakte={data.kontakte}
				title="Wohnungen"
				rows={data.a.wohnungen}
			/>
			<WalterVertraege
				wohnungen={data.wohnungen}
				kontakte={data.kontakte}
				title="Verträge"
				rows={data.a.vertraege}
			/>
		{/await}
	</Accordion>
</WalterGrid>
