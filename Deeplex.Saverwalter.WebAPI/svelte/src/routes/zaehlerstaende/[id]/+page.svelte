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
	const url = `/api/zaehlerstaende/${data.id}`;

	const a: Promise<WalterZaehlerstandEntry> = walter_get(url);
	const entry: Partial<WalterZaehlerstandEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.zaehler.text + ' - ' + convertDate(x.datum));
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterZaehlerstand {a} {entry} />
	{#await a}
		<ButtonSkeleton />
	{:then x}
		<Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
	{/await}
</WalterGrid>
