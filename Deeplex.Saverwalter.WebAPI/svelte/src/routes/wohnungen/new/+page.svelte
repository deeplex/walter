<script lang="ts">
	import { WalterGrid, WalterHeaderNew, WalterWohnung } from '$components';
	import type { WohnungEntry } from '$types';

	const url = `/api/wohnungen`;
	const entry: Partial<WohnungEntry> = {};
	const title = 'Neue Wohnung';
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterWohnung {entry} />
</WalterGrid>
