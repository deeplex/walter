<script lang="ts">
	import { WalterGrid, WalterUmlage, WalterHeaderNew } from '$WalterComponents';
	import type { WalterUmlageEntry } from '$WalterTypes';
	import type { PageData } from './$types';

	const entry: Partial<WalterUmlageEntry> = {};

	export let data: PageData;
</script>

<WalterHeaderNew url={data.url} {entry} title={data.title} />

<WalterGrid>
	<WalterUmlage
		wohnungen={data.wohnungen}
		betriebskostentypen={data.betriebskostentypen}
		umlageschluessel={data.umlageschluessel}
		a={entry}
	/>
</WalterGrid>
