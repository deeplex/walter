<script lang="ts">
	import { WalterHeaderDetail, WalterGrid } from '$WalterComponents';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import WalterVertragVersion from '../../../components/details/WalterVertragVersion.svelte';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderDetail
	fileNames={data.anhaenge}
	a={data.a}
	url={data.url}
	title={data.a.vertrag.text}
/>

<WalterGrid>
	<WalterVertragVersion a={data.a} />
	{#await data.a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/vertraege/${data.a.vertrag.id}`}>Zum Vertrag</Button>
	{/await}
</WalterGrid>
