<script lang="ts">
	import { WalterGrid, WalterHeaderNew, WalterMiete } from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterTypes';

	const url = `/api/miete`;
	const title = 'Neue Miete';

	const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterMiete {entry} />
</WalterGrid>
