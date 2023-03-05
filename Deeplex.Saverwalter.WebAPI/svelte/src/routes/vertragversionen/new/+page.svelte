<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterMiete,
		WalterVertragVersion
	} from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterTypes';

	const url = `/api/vertragversionen`;
	const title = 'Vertragsänderung';

	const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterVertragVersion {entry} />
</WalterGrid>
