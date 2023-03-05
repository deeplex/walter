<script lang="ts">
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehlerstand
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import { convertDate } from '$WalterServices/utils';
	import type { WalterZaehlerstandEntry } from '$WalterTypes';
	import { Button, ButtonSkeleton } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;
	const a = data.a;
</script>

<WalterHeaderDetail
	{a}
	url={data.url}
	title={a.zaehler.text + ' - ' + convertDate(a.datum)}
/>

<WalterGrid>
	<WalterZaehlerstand {a} />
	{#await a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
	{/await}
</WalterGrid>
