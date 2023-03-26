<script lang="ts">
	import { WalterGrid, WalterHeaderNew, WalterMiete } from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterLib';

	const apiURL = `/api/miete`;
	const title = 'Neue Miete';

	const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
	<WalterMiete a={entry} />
</WalterGrid>
