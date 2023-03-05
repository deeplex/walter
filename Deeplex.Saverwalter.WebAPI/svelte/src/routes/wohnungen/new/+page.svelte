<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterWohnung
	} from '$WalterComponents';
	import type { WalterWohnungEntry } from '$WalterTypes';

	const url = `/api/wohnungen`;
	const entry: Partial<WalterWohnungEntry> = {};
	const title = 'Neue Wohnung';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterWohnung a={entry} />
</WalterGrid>
