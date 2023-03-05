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
	const a = data.a;

	const title = a.wohnung.text + ' - ' + a.mieter.map((m) => m.name).join(', ');

	const mieteEntry: Partial<WalterMieteEntry> = {
		vertrag: { id: '' + a.id, text: '' },
		zahlungsdatum: toLocaleIsoString(new Date()),
		betrag: a.versionen[a.versionen.length - 1].grundmiete
	};

	const today = new Date();
	const mietminderungEntry: Partial<WalterMietminderungEntry> = {
		vertrag: { id: '' + a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		ende: toLocaleIsoString(new Date(today.setMonth(today.getMonth() + 1)))
	};

	const lastVersion = a.versionen[a.versionen.length - 1];
	const vertragversionEntry: Partial<WalterVertragVersionEntry> = {
		vertrag: { id: '' + a.id, text: '' },
		beginn: toLocaleIsoString(new Date()),
		personenzahl: lastVersion.personenzahl,
		grundmiete: lastVersion.grundmiete
	};
</script>

<WalterHeaderDetail {a} url={data.url} {title} />

<WalterGrid>
	<WalterVertrag {a} />

	<Accordion>
		<WalterKontakte title="Mieter" rows={a.mieter} />
		<WalterVertragVersionen
			a={vertragversionEntry}
			title="Vertragsänderungen"
			rows={a.versionen}
		/>
		<WalterMieten a={mieteEntry} title="Mieten" rows={a.mieten} />
		<WalterMietminderungen
			a={mietminderungEntry}
			title="Mietminderungen"
			rows={a.mietminderungen}
		/>
	</Accordion>
</WalterGrid>
