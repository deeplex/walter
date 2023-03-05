<script lang="ts">
	import {
		WalterGrid,
		WalterVertrag,
		WalterHeaderNew
	} from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';
	import type { PageData } from './$types';

	export let data: PageData;

	const entry: Partial<WalterVertragEntry> = {};
</script>

<WalterHeaderNew url={data.url} {entry} title={data.title} />

<WalterGrid>
	<WalterVertrag
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		a={entry}
	/>
</WalterGrid>
