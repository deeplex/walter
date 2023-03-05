<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderDetail,
		WalterWohnungen
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterAdresseEntry, WalterWohnungEntry } from '$WalterTypes';
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const url = `/api/adressen/${data.id}`;

	const a: Promise<WalterAdresseEntry> = walter_get(url);
	let entry: Partial<WalterAdresseEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const wohnungEntry: Promise<Partial<WalterWohnungEntry>> = a.then((e) => ({
		adresse: { ...e }
	}));
</script>

<WalterHeaderDetail {a} {url} {entry} title={a.then((x) => x.anschrift)} />

<WalterGrid>
	<WalterAdresse adresse={a} bind:entry />

	<Accordion>
		<WalterWohnungen
			a={wohnungEntry}
			title="Wohnungen"
			rows={a.then((x) => x.wohnungen)}
		/>
	</Accordion>
</WalterGrid>
