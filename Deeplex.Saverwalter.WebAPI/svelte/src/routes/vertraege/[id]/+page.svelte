<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterKontakte,
		WalterMieten,
		WalterMietminderungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterVertrag,
		WalterVertragVersionen
	} from '$WalterComponents';
	import type {
		WalterMieteEntry,
		WalterMietminderungEntry,
		WalterVertragVersionEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;

	const mieteEntry: Partial<WalterMieteEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		zahlungsdatum: toLocaleIsoString(new Date()),
		betrag: data.a.versionen[data.a.versionen.length - 1].grundmiete
	};

	const today = new Date();
	const mietminderungEntry: Partial<WalterMietminderungEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		ende: toLocaleIsoString(new Date(today.setMonth(today.getMonth() + 1)))
	};

	const lastVersion = data.a.versionen[data.a.versionen.length - 1];
	const vertragversionEntry: Partial<WalterVertragVersionEntry> = {
		vertrag: { id: '' + data.a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		personenzahl: lastVersion.personenzahl,
		grundmiete: lastVersion.grundmiete
	};
</script>

<WalterHeaderDetail
	a={data.a}
	url={data.url}
	title={data.a.wohnung.text +
		' - ' +
		data.a.mieter.map((m) => m.name).join(', ')}
/>

<WalterGrid>
	<WalterVertrag
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		a={data.a}
	/>

	<Accordion>
		<WalterKontakte title="Mieter" rows={data.a.mieter} />
		<WalterVertragVersionen
			a={vertragversionEntry}
			title="Vertragsänderungen"
			rows={data.a.versionen}
		/>
		<WalterMieten a={mieteEntry} title="Mieten" rows={data.a.mieten} />
		<WalterMietminderungen
			a={mietminderungEntry}
			title="Mietminderungen"
			rows={data.a.mietminderungen}
		/>
	</Accordion>
</WalterGrid>
