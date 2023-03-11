<script lang="ts">
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehlerstand
	} from '$WalterComponents';
	import { convertDate } from '$WalterServices/utils';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderDetail
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
	title={data.a.zaehler.text + ' - ' + convertDate(data.a.datum)}
/>

<WalterGrid>
	<WalterZaehlerstand a={data.a} />
	{#await data.a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
	{/await}
</WalterGrid>
