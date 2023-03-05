<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehler,
		WalterZaehlerstaende,
		WalterZaehlerList
	} from '$WalterComponents';
	import type {
		WalterZaehlerEntry,
		WalterZaehlerstandEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;

	const lastZaehlerstand =
		data.a.staende[data.a.staende.length - 1] || undefined;
	const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
		zaehler: { id: '' + data.a.id, text: data.a.kennnummer },
		datum: toLocaleIsoString(new Date()),
		stand: lastZaehlerstand.stand || 0,
		einheit: lastZaehlerstand.einheit
	};

	const einzelzaehlerEntry: Partial<WalterZaehlerEntry> = {
		adresse: { ...data.a.adresse },
		typ: data.a.typ,
		allgemeinZaehler: { id: '' + data.a.id, text: data.a.kennnummer }
	};
</script>

<WalterHeaderDetail a={data.a} url={data.url} title={data.a.kennnummer} />

<WalterGrid>
	<WalterZaehler a={data.a} />

	<Accordion>
		<WalterZaehlerstaende
			a={zaehlerstandEntry}
			title="Zählerstände"
			rows={data.a.staende}
		/>
		<WalterZaehlerList
			a={einzelzaehlerEntry}
			title="Einzelzähler"
			rows={data.a.einzelzaehler}
		/>
	</Accordion>
</WalterGrid>
