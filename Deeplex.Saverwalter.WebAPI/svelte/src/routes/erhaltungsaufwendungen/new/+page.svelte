<script lang="ts">
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';
	import {
		WalterGrid,
		WalterErhaltungsaufwendung,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
	const entry: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
	<WalterErhaltungsaufwendung
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		a={entry}
	/>
</WalterGrid>
