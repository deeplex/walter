<script lang="ts">
	import { WalterHeaderDetail, WalterGrid } from '$WalterComponents';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import WalterVertragVersion from '../../../components/details/WalterVertragVersion.svelte';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderDetail
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
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
