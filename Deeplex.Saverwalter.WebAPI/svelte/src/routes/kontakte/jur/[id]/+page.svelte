<script lang="ts">
	import { Accordion, MultiSelect, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import {
		WalterKontakte,
		WalterPerson,
		WalterGrid,
		WalterTextInput,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail,
		WalterMultiSelect
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
		<WalterTextInput labelText="Bezeichnung" value={data.a.name} />
	</Row>
	<WalterPerson juristischePersonen={data.juristischePersonen} value={data.a}>
		<WalterMultiSelect
			titleText="Mitglieder"
			a={data.kontakte}
			bind:value={data.a.selectedMitglieder}
		/>
	</WalterPerson>

	<Accordion>
		<!-- TODO add -->
		<WalterKontakte
			juristischePersonen={data.juristischePersonen}
			title="Mitglieder"
			rows={data.a.mitglieder}
		/>
		<!-- TODO add -->
		<WalterKontakte
			juristischePersonen={data.juristischePersonen}
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
