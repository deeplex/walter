<script lang="ts">
	import { goto } from '$app/navigation';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterMiete
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterMieteEntry } from '$WalterTypes';
	import { Button } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const url = `/api/mieten/${data.id}`;

	const a: Promise<WalterMieteEntry> = walter_get(url);
	const entry: Partial<WalterMieteEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.vertrag.text);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterMiete {a} {entry} />
	{#await a then x}
		<Button on:click={() => goto(`/vertraege/${x.vertrag.id}`)}
			>Zum Vertrag</Button
		>
	{/await}
</WalterGrid>
