<script lang="ts">
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterMiete
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type {
		WalterMieteEntry,
		WalterVertragVersionEntry
	} from '$WalterTypes';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import WalterVertragVersion from '../../../components/details/WalterVertragVersion.svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const url = `/api/vertragversionen/${data.id}`;

	const a: Promise<WalterVertragVersionEntry> = walter_get(url);
	const entry: Partial<WalterVertragVersionEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.vertrag.text);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterVertragVersion {a} {entry} />
	{#await a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/vertraege/${x.vertrag.id}`}>Zum Vertrag</Button>
	{/await}
</WalterGrid>
