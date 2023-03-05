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
</script>

<WalterHeaderDetail a={data.a} url={data.url} title={data.a.name} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={data.a.name} />
	</Row>
	<WalterPerson value={data.a} />

	<Accordion>
		<!-- TODO add -->
		<WalterKontakte title="Mitglieder" rows={data.a.mitglieder} />
		<!-- TODO add -->
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
