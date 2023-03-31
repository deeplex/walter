<script lang="ts">
	import {
		WalterGrid,
		WalterHeaderNew,
		WalterMiete,
		WalterVertragVersion
	} from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterLib';

	const apiURL = `/api/vertragversionen`;
	const title = 'Vertragsänderung';

	const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
	<WalterVertragVersion a={entry} />
</WalterGrid>
