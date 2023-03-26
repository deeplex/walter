<script lang="ts">
	import { Accordion, MultiSelect, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import {
		WalterKontakte,
		WalterGrid,
		WalterWohnungen,
		WalterVertraege,
		WalterHeaderDetail,
		WalterJuristischePerson
	} from '$WalterComponents';
	import type {
		WalterJuristischePersonEntry,
		WalterNatuerlichePersonEntry
	} from '$WalterLib';

	export let data: PageData;

	let mitglied: Partial<
		WalterNatuerlichePersonEntry | WalterJuristischePersonEntry
	> = {
		selectedJuristischePersonen: [{ id: +data.id, text: data.a.name }]
	};

	let juristischePerson: Partial<WalterJuristischePersonEntry> = {
		selectedMitglieder: [{ id: +data.id, text: data.a.name }]
	};
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
	title={data.a.name}
/>

<WalterGrid>
	<WalterJuristischePerson
		a={data.a}
		kontakte={data.kontakte}
		juristischePersonen={data.juristischePersonen}
	/>

	<Accordion>
		<WalterKontakte
			bind:a={mitglied}
			title="Mitglieder"
			rows={data.a.mitglieder}
		/>
		<WalterKontakte
			bind:a={juristischePerson}
			title="Juristische Personen"
			rows={data.a.juristischePersonen}
		/>
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
	</Accordion>
</WalterGrid>
