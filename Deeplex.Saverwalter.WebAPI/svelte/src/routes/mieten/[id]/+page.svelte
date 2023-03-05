<script lang="ts">
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterMiete
	} from '$WalterComponents';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const a = data.a;
</script>

<WalterHeaderDetail {a} url={data.url} title={a.vertrag.text} />

<WalterGrid>
	<WalterMiete {a} />
	{#await a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/vertraege/${x.vertrag.id}`}>Zum Vertrag</Button>
	{/await}
</WalterGrid>
