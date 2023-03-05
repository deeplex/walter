<script lang="ts">
	import { WalterHeaderDetail, WalterGrid } from '$WalterComponents';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import WalterVertragVersion from '../../../components/details/WalterVertragVersion.svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const a = data.a;
</script>

<WalterHeaderDetail {a} url={data.url} title={a.vertrag.text} />

<WalterGrid>
	<WalterVertragVersion {a} />
	{#await a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/vertraege/${a.vertrag.id}`}>Zum Vertrag</Button>
	{/await}
</WalterGrid>
