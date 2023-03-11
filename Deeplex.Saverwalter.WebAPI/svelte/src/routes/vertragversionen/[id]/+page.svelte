<script lang="ts">
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterVertragVersion
	} from '$WalterComponents';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderDetail
	S3URL={data.apiURL}
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
