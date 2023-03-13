<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderDetail,
		WalterKontakte,
		WalterWohnungen,
		WalterZaehlerList
	} from '$WalterComponents';
	import type { WalterWohnungEntry } from '$WalterTypes';
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const wohnungEntry: Partial<WalterWohnungEntry> = { adresse: { ...data.a } };
</script>

<WalterHeaderDetail
	S3URL={data.S3URL}
	a={data.a}
	apiURL={data.apiURL}
	title={data.a.anschrift}
/>

<WalterGrid>
	<WalterAdresse bind:value={data.a} />

	<Accordion>
		<WalterWohnungen
			kontakte={data.kontakte}
			a={wohnungEntry}
			title="Wohnungen"
			rows={data.a.wohnungen}
		/>
		<WalterKontakte
			juristischePersonen={data.juristischePersonen}
			title="Personen"
			rows={data.a.kontakte}
		/>
		<WalterZaehlerList
			zaehler={data.zaehler}
			zaehlertypen={data.zaehlertypen}
			wohnungen={data.wohnungen}
			title="Zähler"
			rows={data.a.zaehler}
		/>
	</Accordion>
</WalterGrid>
