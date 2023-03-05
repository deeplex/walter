<script lang="ts">
	import {
		WalterGrid,
		WalterZaehler,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterZaehlerEntry } from '$WalterTypes';
	import type { PageData } from './$types';

	export let data: PageData;

	const entry: Partial<WalterZaehlerEntry> = {};
</script>

<WalterHeaderNew url={data.url} {entry} title={data.title} />

<WalterGrid>
	<WalterZaehler
		wohnungen={data.wohnungen}
		zaehler={data.zaehler}
		zaehlertypen={data.zaehlertypen}
		a={entry}
	/>
</WalterGrid>
