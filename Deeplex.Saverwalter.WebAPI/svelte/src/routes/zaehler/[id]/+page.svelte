<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehler,
		WalterZaehlerstaende,
		WalterZaehlerList,
		WalterZaehlerstand
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type {
		WalterZaehlerEntry,
		WalterZaehlerstandEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;
	const a = data.a;

	const lastZaehlerstand = a.staende[a.staende.length - 1] || undefined;
	const zaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {
		zaehler: { id: '' + a.id, text: a.kennnummer },
		datum: toLocaleIsoString(new Date()),
		stand: lastZaehlerstand.stand || 0,
		einheit: lastZaehlerstand.einheit
	};

	const einzelzaehlerEntry: Partial<WalterZaehlerEntry> = {
		adresse: { ...a.adresse },
		typ: a.typ,
		allgemeinZaehler: { id: '' + a.id, text: a.kennnummer }
	};
</script>

<WalterHeaderDetail {a} url={data.url} title={a.kennnummer} />

<WalterGrid>
	<WalterZaehler {a} />

	<Accordion>
		<WalterZaehlerstaende
			a={zaehlerstandEntry}
			title="Zählerstände"
			rows={a.staende}
		/>
		<WalterZaehlerList
			a={einzelzaehlerEntry}
			title="Einzelzähler"
			rows={a.einzelzaehler}
		/>
	</Accordion>
</WalterGrid>
